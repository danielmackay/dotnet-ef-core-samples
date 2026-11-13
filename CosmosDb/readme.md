# Cosmos DB

EF Core's Azure Cosmos DB provider stores each entity as a JSON document. This sample shows the EF Core 11 additions for Cosmos: complex types embedded in the document, an indexing policy configured from the model, transactional batches, bulk execution and session tokens.

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

## Transactional batches

`SaveChangesAsync()` groups its writes by container and partition key and sends each group as a Cosmos transactional batch (up to 100 operations). A batch is atomic: if one operation fails, none of them are applied. `DatabaseFacade.AutoTransactionBehavior` decides how EF uses batches:

| Option | Behaviour |
| --- | --- |
| `Auto` (default) | Uses a transactional batch per container and partition. Each batch is atomic, but a save that spans partitions isn't atomic as a whole. |
| `Never` | No batches. Each write is a separate request, so writes that succeeded stay written when a later one fails. |
| `Always` | Throws `InvalidOperationException` before sending anything unless every change fits one batch (same container and partition, at most 100 entities). |

```csharp
db.Database.AutoTransactionBehavior = AutoTransactionBehavior.Always;
db.Customers.AddRange(customerInPartitionA, customerInPartitionB);
await db.SaveChangesAsync();   // InvalidOperationException: ... must be in the same collection and partition ...
```

The sample checks each option:

- **Auto**: two inserts into one partition go out as one batch (counted from the `CosmosEventId.ExecutedTransactionalBatch` event). A batch where one insert conflicts with an existing document fails, and the other insert is rolled back.
- **Never**: the same conflicting change set also fails, but the other document is written, and no batch is used.
- **Always**: a save spanning two partitions throws without writing anything. A single-partition save goes through as one batch.

## Bulk execution

Bulk execution passes the writes to the Cosmos SDK's bulk mode, which groups many operations into fewer requests for better throughput. You turn it on in the Cosmos options:

```csharp
optionsBuilder.UseCosmos(connectionString, databaseName, cosmos => cosmos.BulkExecutionAllowed());

db.Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;   // required, see below
db.Customers.AddRange(manyCustomers);
await db.SaveChangesAsync();
```

The EF Core 11 docs and what's-new page call this option `BulkExecutionEnabled()`, but in EF Core 11.0.0 the method is `BulkExecutionAllowed()`.

Bulk mode and transactional batches are mutually exclusive. With the default `AutoTransactionBehavior.Auto`, EF would use batches and skip bulk mode, so `SaveChangesAsync()` throws for the `CosmosEventId.BulkExecutionWithTransactionalBatch` warning (an error by default). Set `AutoTransactionBehavior.Never`, or ignore the warning if you meant to use batches. The sample checks that the warning is thrown, then bulk-inserts 500 documents and checks that all 500 are there.

## Session tokens

Cosmos's default consistency level, session consistency, guarantees read-your-writes only within a session, identified by a session token. One `DbContext` is one session. When a write and a later read happen in different contexts (say, two requests on different app instances), EF Core 11 lets you pass the token across:

```csharp
// Context 1 writes and reads its token.
await using var writer = new ApplicationDbContext(capture,
    cosmos => cosmos.SessionTokenManagementMode(SessionTokenManagementMode.SemiAutomatic));
writer.Customers.Add(customer);
await writer.SaveChangesAsync();
var token = writer.Database.GetSessionToken();        // e.g. "0:0#1560"

// Context 2 uses it before reading.
await using var reader = new ApplicationDbContext(capture,
    cosmos => cosmos.SessionTokenManagementMode(SessionTokenManagementMode.Manual));
reader.Database.UseSessionToken(token);
var found = await reader.Customers.FindAsync(customer.Id, customer.Region);
```

`SessionTokenManagementMode` options:

| Mode | Behaviour |
| --- | --- |
| `FullyAutomatic` (default) | The Cosmos SDK manages tokens. EF doesn't track them, and `GetSessionToken()` / `UseSessionToken()` throw. |
| `SemiAutomatic` | EF tracks the tokens Cosmos returns, so you can read them. The SDK's automatic handling applies unless you call `UseSessionToken()` for a container. |
| `Manual` | Only the tokens you set with `UseSessionToken()` are used. With none set, no token is sent. EF still tracks returned tokens. |
| `EnforcedManual` | Like `Manual`, but a read throws if `UseSessionToken()` wasn't called first. |

`GetSessionToken()` and `UseSessionToken()` work on the model's default container. That's the context class name unless you call `modelBuilder.HasDefaultContainer(...)`, so the sample sets it to `Customers` (otherwise: "The container with the name 'ApplicationDbContext' does not exist"). For several containers, use `GetSessionTokens()` / `UseSessionTokens()`, which take a dictionary keyed by container name.

The sample checks that `GetSessionToken()` throws in `FullyAutomatic` mode, that the writing context exposes a token, and that a second context using that token reads the new document.

## Emulator limitations

The Linux emulator doesn't support everything Azure Cosmos DB does. Where that affects the sample, it still shows the configuration and records the check as skipped (a `SKIP:` line, counted in the summary):

- **Composite indexes over array paths** (`/Orders/[]/Total`): the emulator rejects the whole container ("The indexing policy is invalid or not supported"). The sample probes for this at startup (`EmulatorCapabilities`) and leaves that one index out of the model on the emulator. Only that "indexing policy is invalid" response counts as unsupported; any other error from the probe fails the sample. Against Azure Cosmos DB the index is created and checked.
- **Multi-property `ORDER BY` without a composite index**: Azure Cosmos DB fails the query, but the emulator runs it anyway. The sample's `ORDER BY` check therefore only checks the results, not that the index was used.

Transactional batches (including rollback on a conflict), bulk execution and session tokens all work on the emulator, so those checks are never skipped. Two caveats:

- The emulator runs a single replica, so a read without the session token would also see the new document. The session-token check proves that the token flows from one context to the other and that the read works, not that it would fail without the token. On a multi-region Azure account, that's where the token matters.
- Bulk-mode throughput on the emulator says nothing about Azure. The sample prints the elapsed time only as a point of interest.

## Resources

- [What's new in EF Core 11: Cosmos DB](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/whatsnew#azure-cosmos-db)
- [EF Core Azure Cosmos DB provider](https://learn.microsoft.com/en-us/ef/core/providers/cosmos/)
- [Complex types](https://learn.microsoft.com/en-us/ef/core/modeling/complex-types)
- [Saving data with the Cosmos provider: transactional batches, bulk execution, session tokens](https://learn.microsoft.com/en-us/ef/core/providers/cosmos/saving)
- [Cosmos DB indexing policies](https://learn.microsoft.com/en-us/azure/cosmos-db/index-policy)
- [Linux-based (vNext) emulator](https://learn.microsoft.com/en-us/azure/cosmos-db/emulator-linux)
