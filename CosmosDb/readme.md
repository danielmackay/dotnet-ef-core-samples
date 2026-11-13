# Cosmos DB

EF Core's Azure Cosmos DB provider stores each entity as a JSON document. This sample shows the EF Core 11 additions for Cosmos: complex types embedded in the document, and an indexing policy configured from the model.

It runs against the Cosmos DB emulator that the AppHost starts (the Linux-based vNext emulator, which runs natively on ARM64). It connects through `DbConnectionFactory.CreateCosmos()` in Common, over plain HTTP on port `8081`. The emulator's Data Explorer is on http://localhost:1234.

Like the other EF Core 11 samples, it checks its own results and exits with a non-zero code if a check fails.

## Async only

EF Core 11 removed synchronous I/O for Cosmos: `EnsureCreated()`, `SaveChanges()`, `ToList()` and friends throw. The sample uses `EnsureCreatedAsync()`, `SaveChangesAsync()`, `ToListAsync()` and so on throughout.

## Complex types

Complex types have no key and value semantics. In Cosmos they're stored inside the owning document: a complex property becomes a nested JSON object, and a complex collection becomes a JSON array of objects.

```csharp
customer.ComplexProperty(c => c.ShippingAddress);   // Address, with a nested Geo complex type
customer.ComplexCollection(c => c.Orders);          // List<Order>
```

```json
{
  "$type": "Customer",
  "id": "alice",
  "Region": "AU",
  "Name": "Alice",
  "Notes": "Prefers morning deliveries",
  "Orders": [
    { "Number": "A-1", "Status": "Shipped", "Total": 120.5 },
    { "Number": "A-2", "Status": "Pending", "Total": 35 }
  ],
  "ShippingAddress": {
    "City": "Sydney", "Postcode": "2000", "Street": "1 George St",
    "Location": { "Latitude": -33.86, "Longitude": 151.21 }
  }
}
```

Queries can filter on nested properties and inside the array:

```csharp
db.Customers.Where(c => c.ShippingAddress.City == "Sydney")        // WHERE c["ShippingAddress"]["City"] = "Sydney"
db.Customers.Where(c => c.Orders.Any(o => o.Status == "Pending"))  // WHERE EXISTS (SELECT 1 FROM o IN c["Orders"] ...)
```

Records work well as complex types: change a value with `address with { Street = ... }` or add to the collection, and `SaveChangesAsync()` replaces the document. `Address` uses init-only properties rather than a positional record, because EF can't bind a nested complex type (`Location`) to a constructor parameter.

## Indexing policy

Cosmos indexes every path by default. The model keeps that, excludes a path that's never queried, and adds composite indexes:

```csharp
customer.HasAutomaticIndexing().Except("/Notes/?");                       // excluded path
customer.HasIndex(c => new { c.ShippingAddress.City, c.Name });           // composite index over a complex property
customer.HasIndex("Region", "Orders[].Total").IsDescending(false, true);  // composite index over a complex collection
```

A multi-property `HasIndex` becomes a Cosmos composite index. Composite indexes are what let Cosmos `ORDER BY` more than one property. Index properties can live inside complex types, and inside complex collections using `[]`, which maps to the `/Orders/[]/Total` path.

The sample reads the container's indexing policy back through the Cosmos SDK (`db.Database.GetCosmosClient()`) and checks that the excluded path and the composite indexes are there.

Note that `Except` takes a raw Cosmos path, so it uses the JSON property names. A single-property `HasIndex` is ignored while automatic indexing is on (everything is indexed already). With `HasAutomaticIndexing(false)`, single-property indexes become the included paths instead.

## Emulator limitations

The Linux emulator doesn't support everything Azure Cosmos DB does. Where that affects the sample, it still shows the configuration, skips the check and prints a `SKIP:` line:

- **Composite indexes over array paths** (`/Orders/[]/Total`): the emulator rejects the whole container ("The indexing policy is invalid or not supported"). The sample probes for this at startup (`EmulatorCapabilities`) and leaves that one index out of the model on the emulator. Against Azure Cosmos DB the index is created and checked.
- **Multi-property `ORDER BY` without a composite index**: Azure Cosmos DB fails the query, but the emulator runs it anyway. The sample's `ORDER BY` check therefore only checks the results, not that the index was used.

## Resources

- [What's new in EF Core 11: Cosmos DB](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/whatsnew#azure-cosmos-db)
- [EF Core Azure Cosmos DB provider](https://learn.microsoft.com/en-us/ef/core/providers/cosmos/)
- [Complex types](https://learn.microsoft.com/en-us/ef/core/modeling/complex-types)
- [Cosmos DB indexing policies](https://learn.microsoft.com/en-us/azure/cosmos-db/index-policy)
- [Linux-based (vNext) emulator](https://learn.microsoft.com/en-us/azure/cosmos-db/emulator-linux)
