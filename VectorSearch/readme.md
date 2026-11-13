# Vector Search

EF Core 11 adds approximate nearest-neighbour search on SQL Server through the new `VECTOR_SEARCH()` function and vector indexes. This sample needs SQL Server 2025, which the AppHost provides.

> **Experimental.** `VECTOR_SEARCH()` and vector indexes are preview features in SQL Server and experimental APIs in EF Core 11. `HasVectorIndex`, `VectorSearch`, `WithApproximate` and `VectorSearchResult<T>` raise compile error `EF9105` until you opt in, which the project does with `<NoWarn>$(NoWarn);EF9105</NoWarn>`. The API or the SQL it generates may still change.

## The data

`Article` has a `SqlVector<float>` property mapped to a `vector(4)` column. Real embeddings come from an embedding model and have hundreds or thousands of dimensions. This sample hand-picks 4-dimensional embeddings in `SampleArticles.cs`, with one dimension per topic (`[databases, cooking, travel, space]`). That way it runs offline, calls no AI service, and returns the same results on every run.

The search vector stands for the question "how do I make my database queries faster?" (`[1, 0, 0, 0.05]`), so the nearest articles are the three about databases.

The `Embedding` property is a non-nullable `SqlVector<float>`. `VectorSearch()` doesn't accept a nullable vector property (`CS8377`).

## A vector index created by migrations

```csharp
modelBuilder.Entity<Article>()
    .Property(a => a.Embedding)
    .HasColumnType("vector(4)");

modelBuilder.Entity<Article>()
    .HasVectorIndex(a => a.Embedding)
    .HasMetric("cosine");
```

The second argument of `HasVectorIndex` is the index *name*, not the metric. Pass the metric with `HasMetric`.

The `AddVectorIndex` migration creates:

```sql
CREATE VECTOR INDEX [IX_Articles_Embedding] ON [Articles]([Embedding]) WITH (METRIC = 'cosine');
```

On SQL Server 2025 there are two catches, both handled in the sample:

- Vector indexes are a preview feature, so the database needs `PREVIEW_FEATURES` turned on first. The migration has a hand-added `ALTER DATABASE SCOPED CONFIGURATION SET PREVIEW_FEATURES = ON;`, run with `suppressTransaction: true` because that statement can't run inside a transaction.
- A table with a vector index becomes **read-only**. Inserts fail with *Data modification statement failed because table 'Articles' has a vector index on it*. The sample therefore runs `Migrate("InitialCreate")`, inserts the articles, and only then runs `Migrate()` to add the index. Azure SQL Database's newer vector indexes don't have this limit.

The sample checks that `sys.vector_indexes` lists `IX_Articles_Embedding`.

## Approximate search

```csharp
var results = await db.Articles
    .VectorSearch(a => a.Embedding, query, "cosine")
    .OrderBy(r => r.Distance)
    .Take(3)
    .WithApproximate()
    .ToListAsync();

foreach (var (article, distance) in results)
    Console.WriteLine($"{distance:F4} {article.Title}");
```

Each result is a `VectorSearchResult<Article>` with the entity in `Value` and the distance in `Distance`. `WithApproximate()` must come after `Take()`. The metric must match the index's metric.

```sql
SELECT TOP(@p1) WITH APPROXIMATE [a].[Id], [a].[Body], [a].[Title], [v].[Distance]
FROM VECTOR_SEARCH(
    TABLE = [Articles] AS [a],
    COLUMN = [Embedding],
    SIMILAR_TO = @p,
    METRIC = 'cosine'
) AS [v]
ORDER BY [v].[Distance]
```

## Exact search

Leave out `WithApproximate()` and SQL Server compares the query vector with every row (`TOP(@p1)` without `WITH APPROXIMATE`). That always finds the true nearest rows, but it's slower on big tables. EF logs warning `VectorSearchWithoutApproximateIndexWarning` to remind you.

The other way to run an exact search is EF Core 10's `EF.Functions.VectorDistance`, which works on every SQL Server 2025 build:

```csharp
db.Articles
    .Select(a => new { a.Title, Distance = EF.Functions.VectorDistance("cosine", a.Embedding, query) })
    .OrderBy(a => a.Distance)
    .Take(3)
```

```text
0.0012  Querying JSON in SQL Server
0.0029  Tuning SQL Server indexes
0.0750  Vector databases explained
```

## Running `VECTOR_SEARCH()` on SQL Server 2025

EF Core 11 generates the **latest** `VECTOR_SEARCH()` syntax (`TOP(n) WITH APPROXIMATE` outside the function). Azure SQL Database, SQL database in Fabric and Azure SQL Managed Instance (always-up-to-date) support it. Boxed SQL Server 2025 (up to CU11 at the time of writing) only supports the earlier syntax with `TOP_N` inside `VECTOR_SEARCH()`, so it rejects both queries with *Incorrect syntax near 'APPROXIMATE'* or *Incorrect syntax near 'TOP_N'*.

So on the AppHost's SQL Server the sample checks the generated SQL of both queries, prints a `SKIP` line instead of running them, and runs the exact search with `VectorDistance`. Against a server that supports the new syntax, it also runs both `VectorSearch` queries and checks their results.

## Vectors are no longer loaded by default

Since EF Core 11, loading an entity doesn't read its vector columns, because vectors are large and rarely needed once they're stored:

```csharp
var article = db.Articles.Single(a => a.Title == "Querying JSON in SQL Server");
// SELECT TOP(2) [a].[Id], [a].[Body], [a].[Title] FROM [Articles] ...
// article.Embedding is default(SqlVector<float>): Length == 0
```

`SqlVector<float>` is a struct, so the unloaded property isn't a C# `null`. It's left at `default(SqlVector<float>)`, which has no values. Check `Length == 0` rather than `IsNull`: in SqlClient 7.1, `IsNull` is `false` for the default value.

To read a vector, project it explicitly:

```csharp
db.Articles.Select(a => new { a.Title, a.Embedding })
// SELECT TOP(2) [a].[Title], [a].[Embedding] FROM [Articles] ...
```

## Self-checks

The sample checks that:

- the vector index exists in `sys.vector_indexes`
- the approximate query's SQL contains `VECTOR_SEARCH` and `WITH APPROXIMATE`, and the exact query's SQL leaves out `WITH APPROXIMATE`
- the approximate and exact searches return the three database articles, nearest first, with distances in ascending order (the `VectorSearch` result checks only run on servers that support the new syntax; see above)
- a loaded entity's vector is empty and a projection returns it

It exits with a non-zero code if any check fails.

## Resources

- [What's new in EF Core 11 | Vector search](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/whatsnew#sqlserver-vector-search)
- [Vector search in the SQL Server EF Core provider](https://learn.microsoft.com/en-us/ef/core/providers/sql-server/vector-search)
- [VECTOR_SEARCH (Transact-SQL)](https://learn.microsoft.com/en-us/sql/t-sql/functions/vector-search-transact-sql)
- [CREATE VECTOR INDEX (Transact-SQL)](https://learn.microsoft.com/en-us/sql/t-sql/statements/create-vector-index-transact-sql)
