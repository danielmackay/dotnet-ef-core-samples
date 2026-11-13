# Full-Text Search

EF Core 11 adds first-class support for SQL Server Full-Text Search: you declare full-text catalogs and indexes in the model so migrations create them, and query with the `FREETEXTTABLE()` and `CONTAINSTABLE()` table-valued functions, which return a relevance rank for each match.

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

## Checks

The sample checks that:

- Full-Text Search is installed
- the migration script creates the catalog and the full-text index, and both exist in `sys.fulltext_catalogs` / `sys.fulltext_indexes` after `Migrate()`
- the index finishes populating with every row
- the SQL uses `FREETEXTTABLE` / `CONTAINSTABLE`, results come back in descending rank order, and the expected article ranks first

It exits with a non-zero code if any check fails.

## Resources

- [EF Core Docs | Full-Text Search](https://learn.microsoft.com/en-us/ef/core/providers/sql-server/full-text-search)
- [What's new in EF Core 11 | Full-text search](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/whatsnew#sqlserver-full-text)
- [FREETEXTTABLE (Transact-SQL)](https://learn.microsoft.com/en-us/sql/relational-databases/system-functions/freetexttable-transact-sql)
- [CONTAINSTABLE (Transact-SQL)](https://learn.microsoft.com/en-us/sql/relational-databases/system-functions/containstable-transact-sql)
- [Install SQL Server Full-Text Search on Linux](https://learn.microsoft.com/en-us/sql/linux/sql-server-linux-setup-full-text-search)
