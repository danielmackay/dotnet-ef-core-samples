# Full-Text Search

EF Core 11 adds first-class support for SQL Server Full-Text Search: you declare full-text catalogs and indexes in the model so migrations create them, and query with the `FREETEXTTABLE()` and `CONTAINSTABLE()` table-valued functions, which return a relevance rank for each match. The sample ends with hybrid search: full-text and vector search merged with `FullJoin` and scored with reciprocal rank fusion.

This sample needs SQL Server with Full-Text Search installed. The stock Linux image doesn't include it, so the AppHost builds a SQL Server 2025 image with the `mssql-server-fts` package added (see the repo readme, including the notes for Apple Silicon Macs). The sample checks `FULLTEXTSERVICEPROPERTY('IsFullTextInstalled')` first and stops with a `FAIL` line if Full-Text Search is missing.

## Full-text index in the model

```csharp
modelBuilder.Entity<Article>()
    .HasFullTextIndex(a => new { a.Title, a.Contents })
    .UseKeyIndex("PK_Articles")
    .UseCatalog("ftCatalog");
```

`dotnet ef migrations add` turns this into a normal `CreateIndex` with full-text annotations, and the migration runs it as:

```sql
CREATE FULLTEXT INDEX ON [Articles]([Title], [Contents]) KEY INDEX [PK_Articles] ON [ftCatalog];
```

EF runs full-text DDL outside the migration transaction, because SQL Server doesn't allow it inside a user transaction.

Columns in a full-text index are sized like index key columns (`nvarchar(450)`) unless you say otherwise. Full-text search works on max types, so the sample maps `Contents` to `nvarchar(max)` explicitly.

### Known issue: `HasFullTextCatalog` breaks migrations in EF Core 11.0.0

The catalog would normally live in the model too:

```csharp
modelBuilder.HasFullTextCatalog("ftCatalog"); // optionally .IsDefault() / .IsAccentSensitive(false)
```

In EF Core 11.0.0 this works with `EnsureCreated()` but not with migrations:

1. `dotnet ef migrations add` fails with `Cannot scaffold C# literals of type 'SqlServerFullTextCatalog'` ([dotnet/efcore#39599](https://github.com/dotnet/efcore/issues/39599)).
2. If you get past that by customising the migration code generator, the model snapshot generator then fails on `.IsDefault()`, and the model differ treats the catalog as changed every time, because it compares catalog objects by reference. As a result `Migrate()` throws `PendingModelChangesWarning`, and every new migration gets an empty `AlterDatabase` operation.

So the sample leaves the catalog out of the model. The index still names it with `UseCatalog`, and the generated `InitialCreate` migration has one line added by hand:

```csharp
// Up()
migrationBuilder.Sql("CREATE FULLTEXT CATALOG [ftCatalog];", suppressTransaction: true);
// Down(), after dropping the table
migrationBuilder.Sql("DROP FULLTEXT CATALOG [ftCatalog];", suppressTransaction: true);
```

Once the bug is fixed, move the catalog back into the model with `HasFullTextCatalog`.

## Waiting for population

SQL Server fills a full-text index in the background after rows change, so a query straight after `SaveChanges` can miss rows. The sample polls until the catalog is idle and the index contains every row:

```sql
SELECT FULLTEXTCATALOGPROPERTY('ftCatalog', 'PopulateStatus');         -- 0 = idle, 1 = populating, 9 = change tracking
SELECT OBJECTPROPERTYEX(OBJECT_ID('Articles'), 'TableFulltextItemCount'); -- rows in the index
```

## `FreeTextTable`

`FREETEXTTABLE` matches on meaning rather than exact words (stemming and thesaurus), so "growing vegetables" also finds "grow" and "vegetable". It returns `FullTextSearchResult<TKey>` (a `Key` and an integer `Rank`, higher is better), so join back to the table to get the rows:

```csharp
db.Articles
    .Join(db.Articles.FreeTextTable<Article, int>("growing vegetables"),
        a => a.Id, f => f.Key, (a, f) => new { a.Title, f.Rank })
    .OrderByDescending(x => x.Rank)
```

```sql
SELECT [a].[Title], [f].[RANK] AS [Rank]
FROM [Articles] AS [a]
INNER JOIN FREETEXTTABLE([Articles], *, @p) AS [f] ON [a].[Id] = [f].[KEY]
ORDER BY [f].[RANK] DESC
```

The type arguments are needed: C# can't infer the key type. The docs' example selects `r.Value`, but `FullTextSearchResult<TKey>` has no `Value`, only `Key` and `Rank`.

## `ContainsTable`

`CONTAINSTABLE` takes a full-text search condition, so you get prefix terms, `NEAR`, boolean operators and weights. This query searches only `Contents` and weights carrots far above fruit:

```csharp
db.Articles
    .ContainsTable<Article, int>("ISABOUT(carrots WEIGHT(0.9), fruit WEIGHT(0.1))", a => a.Contents)
    .Join(db.Articles, c => c.Key, a => a.Id, (c, a) => new { a.Title, c.Rank })
    .OrderByDescending(x => x.Rank)
```

```sql
SELECT [a0].[Title], [c].[RANK] AS [Rank]
FROM CONTAINSTABLE([Articles], [Contents], @p) AS [c]
INNER JOIN [Articles] AS [a0] ON [c].[KEY] = [a0].[Id]
ORDER BY [c].[RANK] DESC
```

Both methods also take an optional `languageTerm` and a `topN` limit.

## Hybrid search

Full-text search matches words: "vegetables" finds articles that say "vegetables", but not one about eating your greens. Vector search matches meaning: it compares embeddings, so it finds the greens article, but it can miss an article that has the exact word and is about something else. **Hybrid search** runs both searches and merges the results, so an article either search finds is included, and an article both find ranks highest. It's a common way to fetch documents for RAG apps.

The sample stores a 4-dimension `vector(4)` embedding on each `Article`, hand-picked like the VectorSearch sample's (each dimension is a topic: gardening, healthy food, cars, cooking), so it runs offline. It searches for the text "vegetables" and the embedding of "fresh produce from my garden", top 3 from each:

| Article | Full-text position | Vector position | RRF score |
| --- | --- | --- | --- |
| Growing vegetables at home | 1 | 1 | 0.0328 |
| Healthy eating | - | 2 | 0.0161 |
| Winter soup | 2 | - | 0.0161 |
| Fruit salad | - | 3 | 0.0159 |

### Merging with `FullJoin`

EF Core 11's `FullJoin` translates to `FULL JOIN`, so an article found by only one side still comes back, with NULLs for the other side. The two rankings are on different scales (`FREETEXTTABLE`'s `RANK` is an integer where higher is better; a cosine distance runs from 0 to 2 and lower is better), so they can't be added together directly.

### Reciprocal rank fusion (RRF)

RRF solves that by using each article's **position** in each list (1st, 2nd, 3rd...) instead of its rank or distance:

```
score = Σ over the lists that found the article: 1 / (k + position)
```

`k` (60, the value from the original RRF paper) stops the first place from dominating. An article found by both lists gets two terms, so it beats an article that's first in only one list.

EF can't translate `ROW_NUMBER()`, so the sample computes each position as "1 + the number of rows that rank better", with a correlated subquery that EF translates to `COUNT(*)`. The full-text side:

```csharp
db.Articles.Join(
    db.Articles.FreeTextTable<Article, int>("vegetables", topN: 3),
    a => a.Id,
    f => f.Key,
    (a, f) => new RankedHit
    {
        Id = a.Id,
        Title = a.Title,
        Position = 1 + db.Articles.FreeTextTable<Article, int>("vegetables").Count(other => other.Rank > f.Rank),
    });
```

The merge (see `HybridSearch.Fuse`):

```csharp
fullText
    .FullJoin(vector, t => t.Id, v => v.Id, (t, v) => new
    {
        Title = t.Title ?? v.Title ?? string.Empty,
        FullTextPosition = t.Id == null ? null : t.Position,
        VectorPosition = v.Id == null ? null : v.Position,
    })
    .Select(x => new
    {
        x.Title,
        x.FullTextPosition,
        x.VectorPosition,
        Score = (x.FullTextPosition == null ? 0.0 : 1.0 / (60 + x.FullTextPosition.Value))
            + (x.VectorPosition == null ? 0.0 : 1.0 / (60 + x.VectorPosition.Value)),
    })
    .OrderByDescending(x => x.Score)
```

The whole search, both sides plus the merge and the scoring, runs on the server as one SQL statement.

Things to watch for:

- **The docs' scoring isn't true RRF.** The EF Core docs' hybrid example scores `1.0 / (k + f.Rank)` and `1.0 / (k + v.Distance)`, which uses the raw values. A *better* full-text match has a *higher* `RANK`, so it gets a *lower* score, and a distance isn't a position. Use positions.
- **A missing side doesn't give NULL counts.** A `COUNT(*)` subquery correlated to the missing side of the FULL JOIN returns 0, not NULL, so `t.Position` alone would make every article look found by both searches. The sample checks the side's key (`t.Id == null`) instead.
- **Read columns, not entities.** In EF Core 11.0.0, a result selector that projects entities with ternaries (`f != null ? f.Article : v.Article`, as in the docs) generates valid SQL but throws `Cannot read the Value property of a Nullable object that has no value` when one side is missing. Project scalar columns through the `FullJoin` and combine them with `??`.
- **Make the ranked-list type a struct.** `FullJoin` passes `null` for the missing side, so with a class the selector needs `t!.Title` (and `t != null` on a class that isn't an entity can't be translated). `RankedHit` is a `readonly struct`, so the selector gets a `RankedHit` whose columns are all NULL instead, and reads them without `!` or null checks.
- The docs' `vs == null` doesn't compile: `VectorSearchResult<T>` is a struct.

### Why there are two hybrid queries

The EF Core docs' version uses `VectorSearch(...).Take(3).WithApproximate()` for the vector side:

```csharp
db.Articles
    .VectorSearch(a => a.Embedding, query, "cosine")
    .OrderBy(v => v.Distance)
    .Take(3)
    .WithApproximate()
```

EF Core 11 translates this to the latest `VECTOR_SEARCH` syntax, `SELECT TOP(3) WITH APPROXIMATE ... FROM VECTOR_SEARCH(...)`, which Azure SQL Database supports. Boxed SQL Server 2025 (up to CU11 at the time of writing) only has the earlier syntax and rejects it with `Incorrect syntax near 'APPROXIMATE'` (error 102). See the VectorSearch sample for details. So the sample:

1. Builds the `VectorSearch` hybrid query and checks with `ToQueryString()` that its SQL contains `FREETEXTTABLE`, `VECTOR_SEARCH`, `WITH APPROXIMATE` and `FULL JOIN`. Then it tries to run it, and records a skip if the server rejects that syntax (error 102 near `APPROXIMATE`; any other error fails the sample). On a server that supports it, the result checks below run against it too.
2. Runs the same hybrid search with an exact vector search, `EF.Functions.VectorDistance` (EF Core 10, not experimental, needs no vector index), which works on every SQL Server 2025 build. The result checks run on this query. Its SQL is `FREETEXTTABLE ... FULL JOIN (SELECT TOP(3) ... ORDER BY VECTOR_DISTANCE(...))`.

`VectorSearch` and `WithApproximate` are experimental, so the project suppresses `EF9105` (see the csproj). `VectorSearch()` can't be nested in a subquery, so even the `VectorSearch` version counts closer rows with `VECTOR_DISTANCE` to get the vector positions.

### Adding the vector column

The `AddEmbedding` migration adds the `vector(4)` column. EF scaffolded it with `defaultValue: new SqlVector<float>(...)` because the column is `NOT NULL`, but SQL Server can't put a `DEFAULT` constraint on a vector column (error 1752), so that argument was removed by hand. The table is empty when this migration runs; with existing rows, add the column as nullable, fill in the embeddings, then make it `NOT NULL`.

## Checks

The sample checks that:

- Full-Text Search is installed
- the migration script creates the full-text index on `Title` and `Contents`, keyed by `PK_Articles`, in `ftCatalog`
- the migration script still has the `CREATE FULLTEXT CATALOG` line (hand-added to the migration because of EF bugs, see above)
- the catalog and the index exist in `sys.fulltext_catalogs` / `sys.fulltext_indexes` after `Migrate()`
- the index finishes populating with every row
- the SQL uses `FREETEXTTABLE` / `CONTAINSTABLE`, results come back in descending rank order, and the expected article ranks first
- the `VectorSearch` hybrid query's SQL has `FREETEXTTABLE`, `VECTOR_SEARCH ... WITH APPROXIMATE` and `FULL JOIN` (running it is a `SKIP` on a server without that syntax, counted in the summary)
- the runnable hybrid query runs as one SQL statement with `FREETEXTTABLE`, `VECTOR_DISTANCE` and `FULL JOIN`; its results include an article only full-text search finds and one only vector search finds; the article both find ranks highest; and the results are in descending RRF score order

It exits with a non-zero code if any check fails.

## Resources

- [EF Core Docs | Full-Text Search](https://learn.microsoft.com/en-us/ef/core/providers/sql-server/full-text-search)
- [What's new in EF Core 11 | Full-text search](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/whatsnew#sqlserver-full-text)
- [EF Core Docs | Vector search: hybrid search](https://learn.microsoft.com/en-us/ef/core/providers/sql-server/vector-search#hybrid-search)
- [Reciprocal Rank Fusion outperforms Condorcet and individual Rank Learning Methods (Cormack, Clarke, Büttcher, SIGIR 2009)](https://dl.acm.org/doi/10.1145/1571941.1572114)
- [FREETEXTTABLE (Transact-SQL)](https://learn.microsoft.com/en-us/sql/relational-databases/system-functions/freetexttable-transact-sql)
- [CONTAINSTABLE (Transact-SQL)](https://learn.microsoft.com/en-us/sql/relational-databases/system-functions/containstable-transact-sql)
- [Install SQL Server Full-Text Search on Linux](https://learn.microsoft.com/en-us/sql/linux/sql-server-linux-setup-full-text-search)
