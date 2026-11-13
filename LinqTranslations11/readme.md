# LINQ Translations (EF Core 11)

EF Core 11 translates more LINQ to SQL and generates leaner SQL for some common query shapes. This sample runs each one, prints the SQL EF generated, and checks both the results and a distinguishing fragment of the SQL. It exits with a non-zero code if any check fails.

## What it shows

### 1. `FullJoin` → `FULL JOIN`

.NET 11 adds a `FullJoin` LINQ operator, which keeps rows from both sides and matches them when the keys are equal. EF Core 11 translates it to `FULL JOIN`. Both sides of the result selector can be `null`, so the sample uses null checks when projecting.

The sample joins customers to orders. The customer with no orders and the order with no customer both come back.

### 2. `MaxByAsync` / `MinByAsync` → `TOP(1) ... ORDER BY`

`MaxBy`/`MinBy` return the *element* with the largest or smallest key, not just the key. EF Core 11 translates them (and the new `MaxByAsync`/`MinByAsync`) to a single `SELECT TOP(1) ... ORDER BY <key> [DESC]` query, so you no longer write `OrderByDescending(...).FirstOrDefault()` by hand.

### 3. GroupBy improvements

- **Navigation over the top row per group.** `GroupBy(...).Select(g => g.OrderByDescending(...).First())` followed by a navigation (`p.Blog.Name`) used to fail with "Translation of member 'Blog' failed". EF Core 11 expands the navigation into a regular join over the grouped query (the top row is picked with `ROW_NUMBER() OVER(PARTITION BY ...)`).
- **Aggregates over reference navigations.** `g.Sum(p => p.Blog.Rating)` used to translate to a correlated subquery that scanned the table again for every group. EF Core 11 joins `Blogs` before grouping and aggregates the joined column: `SUM([b].[Rating]) ... GROUP BY`.

### 4. Better SQL for to-one joins

- **Split queries.** With `Include(b => b.Owner).Include(b => b.Posts).AsSplitQuery()`, the query that loads `Posts` used to join `People` (the owner) too, and order by its key. EF Core 11 drops that join: the owner was already loaded by the first query.
- **Single queries.** Without `AsSplitQuery()`, EF used to add the owner's key to `ORDER BY` (`ORDER BY [b].[Id], [p].[Id]`). The blog's key already determines the owner through the foreign key, so EF Core 11 leaves it out (`ORDER BY [b].[Id]`).

The EF team measured roughly 29% (split) and 22% (single) faster queries in a simple benchmark.

### 5. No-op `CAST`s removed

`Customer.Email` is an `Email` value object stored as `nvarchar(max)` through a value converter, with an implicit conversion to `string`. Filtering with `((string)c.Email).StartsWith("alice")` used to generate `CAST([c].[Email] AS nvarchar(max)) LIKE ...`, a cast to the type the column already has. Besides being wasted work, a function around a column stops SQL Server from using an index on it. EF Core 11 strips these no-op casts: `[c].[Email] LIKE N'alice%'`.

## Running

Start the AppHost (or any SQL Server on `127.0.0.1,1700`), then:

```bash
dotnet run --project LinqTranslations11
```

## Resources

- [What's new in EF Core 11 | LINQ and SQL translation](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/whatsnew#linq-and-sql-translation)
- [Single vs. split queries](https://learn.microsoft.com/en-us/ef/core/querying/single-split-queries)
- [Value conversions](https://learn.microsoft.com/en-us/ef/core/modeling/value-conversions)
- [dotnet/efcore#38668: GroupBy aggregates over reference navigations](https://github.com/dotnet/efcore/pull/38668)
- [dotnet/efcore#38687: composing over GroupBy + First per group](https://github.com/dotnet/efcore/pull/38687)
