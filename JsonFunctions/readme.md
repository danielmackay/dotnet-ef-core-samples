# JSON Functions

EF Core 11 adds two `EF.Functions` methods for querying JSON on SQL Server, and translates `Contains` over a primitive collection to the new `JSON_CONTAINS` function when the database runs at compatibility level 170 (SQL Server 2025). This sample needs SQL Server 2025, which the AppHost provides.

## `EF.Functions.JsonPathExists`

Filters rows whose JSON document contains a path, which is handy when documents don't all share the same shape:

```csharp
db.Products.Where(p => EF.Functions.JsonPathExists(p.JsonData, "$.OptionalInt"))
```

```sql
WHERE JSON_PATH_EXISTS([p].[JsonData], N'$.OptionalInt') = 1
```

A key whose value is JSON `null` still counts as present, so the sample returns both `Keyboard` (`"OptionalInt": 5`) and `Monitor` (`"OptionalInt": null`).

## `EF.Functions.JsonContains`

Calls SQL Server's `JSON_CONTAINS` directly to look for a value, optionally at a path:

```csharp
db.Products.Where(p => EF.Functions.JsonContains(p.JsonData, 8, "$.Rating") == 1)
```

```sql
WHERE JSON_CONTAINS([p].[JsonData], 8, N'$.Rating') = 1
```

Two things to know:

- `JsonContains` is experimental in EF Core 11, so it raises compile error `EF9106` until you opt in. The project does this with `<NoWarn>$(NoWarn);EF9106</NoWarn>`.
- `JSON_CONTAINS` only accepts the native `json` type. On an `nvarchar(max)` column SQL Server fails with *Argument data type nvarchar(max) is invalid for argument 1 of json_contains function*. A plain `string` property stays `nvarchar(max)` even at compatibility 170, so the sample maps it with `.HasColumnType("json")`.

## `Contains` at compatibility 160 vs 170

The sample runs the same query through two contexts, one at each compatibility level (`UseCompatibilityLevel(160)` and `UseCompatibilityLevel(170)`):

```csharp
db.Products.Where(p => p.Tags.Contains("ef-core"))
```

At compatibility 160 (the EF Core 11 default) EF opens the JSON array with `OPENJSON`:

```sql
WHERE N'ef-core' IN (
    SELECT [t].[value]
    FROM OPENJSON([p].[Tags]) WITH ([value] nvarchar(max) '$') AS [t]
)
```

At compatibility 170 it uses `JSON_CONTAINS`:

```sql
WHERE JSON_CONTAINS([p].[Tags], N'ef-core') = 1
```

Both return the same rows. At compatibility 170 EF also creates primitive collections as `json` columns, which `JSON_CONTAINS` needs.

In EF Core 11.0.0 the `JSON_CONTAINS` translation only happens when the searched value is a constant. A C# variable becomes a SQL parameter and still translates to `OPENJSON`, even at compatibility 170.

## Self-checks

The sample checks the rows returned by `JsonPathExists` and `JsonContains`, that the compatibility 160 SQL contains `OPENJSON` and the 170 SQL contains `JSON_CONTAINS`, and that both levels return the same rows. It exits with a non-zero code if any check fails.

## Resources

- [What's new in EF Core 11 | JSON_CONTAINS](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/whatsnew#sql-server-json-contains)
- [JSON_CONTAINS (Transact-SQL)](https://learn.microsoft.com/en-us/sql/t-sql/functions/json-contains-transact-sql)
- [JSON_PATH_EXISTS (Transact-SQL)](https://learn.microsoft.com/en-us/sql/t-sql/functions/json-path-exists-transact-sql)
