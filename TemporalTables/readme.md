# Temporal Tables

Temporal tables allow you to track the history of data changes in a table.  This works by creating a shadow table that is used to store a copy of the data for every version. The shadow table is automatically updated when the data in the main table is updated.  The shadow table is not visible to the user, but can be queried using EF Core.

## EF Core 11: period columns as CLR properties

Every temporal table has two period columns (`PeriodStart` and `PeriodEnd`) that SQL Server fills in. Up to EF Core 10 they could only be mapped as shadow properties, so reading or querying them meant `EF.Property<DateTime>(p, "PeriodStart")`.

EF Core 11 adds lambda overloads of `HasPeriodStart` / `HasPeriodEnd`, so the columns can be mapped to ordinary properties on the entity:

```csharp
modelBuilder
    .Entity<Product>()
    .ToTable("Products", b => b.IsTemporal(t =>
    {
        t.HasPeriodStart(p => p.PeriodStart);
        t.HasPeriodEnd(p => p.PeriodEnd);
    }));
```

The properties can then be used directly in queries:

```csharp
var history = db.Products
    .TemporalAll()
    .OrderBy(p => p.PeriodStart)
    .Select(p => new { p.Price, p.PeriodStart, p.PeriodEnd })
    .ToList();
```

```sql
SELECT [p].[Price], [p].[PeriodStart], [p].[PeriodEnd]
FROM [Products] FOR SYSTEM_TIME ALL AS [p]
ORDER BY [p].[PeriodStart]
```

The sample checks that the period values are populated, that the history comes back oldest first with each version ending when the next one starts, and that the SQL orders by the period column. It exits with a non-zero code if any check fails.

## Resources

- [EF Core Docs | Temporal Tables](https://learn.microsoft.com/en-us/ef/core/providers/sql-server/temporal-tables)
- [What's new in EF Core 11](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/whatsnew)
