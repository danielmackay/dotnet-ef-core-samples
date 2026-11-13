using Common;
using CosmosDb;

Console.WriteLine("Cosmos DB Sample");

// Decide the model before the first DbContext builds (and caches) it.
ApplicationDbContext.IncludeArrayCompositeIndex = await EmulatorCapabilities.SupportsArrayCompositeIndexAsync();
if (!ApplicationDbContext.IncludeArrayCompositeIndex)
{
    Console.WriteLine("SKIP: this Cosmos endpoint (the Linux emulator) rejects composite indexes over array paths, " +
                      "so the Orders[] composite index is left out of the model. Azure Cosmos DB supports it.");
}

// EF Core 11 removed sync I/O for Cosmos (EnsureCreated, SaveChanges, ToList... throw), so every call is async.
await using (var db = new ApplicationDbContext(new CosmosCapture()))
{
    await db.Database.EnsureDeletedAsync();
    await db.Database.EnsureCreatedAsync();
}

await ComplexTypesDemo.RunAsync();
await IndexingPolicyDemo.RunAsync();

Console.WriteLine();
Check.Summary();

Console.ReadLine();
