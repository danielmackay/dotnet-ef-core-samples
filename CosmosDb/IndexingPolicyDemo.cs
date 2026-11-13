using Common;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;

namespace CosmosDb;

/// <summary>
/// The indexing policy EF builds from the model (see ApplicationDbContext.OnModelCreating), read back
/// from the container through the Cosmos SDK.
/// </summary>
public static class IndexingPolicyDemo
{
    public static async Task RunAsync()
    {
        Console.WriteLine();
        Console.WriteLine("== Indexing policy ==");

        var capture = new CosmosCapture();
        await using var db = new ApplicationDbContext(capture);

        var container = db.Database.GetCosmosClient()
            .GetContainer(ApplicationDbContext.DatabaseName, ApplicationDbContext.CustomersContainer);
        ContainerProperties properties = await container.ReadContainerAsync();
        var policy = properties.IndexingPolicy;

        Console.WriteLine($"Automatic: {policy.Automatic}, mode: {policy.IndexingMode}");
        Console.WriteLine($"Included:  {string.Join(", ", policy.IncludedPaths.Select(p => p.Path))}");
        Console.WriteLine($"Excluded:  {string.Join(", ", policy.ExcludedPaths.Select(p => p.Path))}");
        var composites = policy.CompositeIndexes
            .Select(index => string.Join(", ", index.Select(p => $"{p.Path} {p.Order}")))
            .ToList();
        composites.ForEach(c => Console.WriteLine($"Composite: ({c})"));

        Check.That(policy.IncludedPaths.Any(p => p.Path == "/*"), "automatic indexing still covers every other path");
        Check.That(policy.ExcludedPaths.Any(p => p.Path == "/Notes/?"), "excluded path /Notes/? is in the policy");
        Check.That(composites.Contains("/ShippingAddress/City Ascending, /Name Ascending"),
            "composite index over a nested complex property is in the policy");
        if (ApplicationDbContext.IncludeArrayCompositeIndex)
        {
            Check.That(composites.Contains("/Region Ascending, /Orders/[]/Total Descending"),
                "composite index over a complex collection path is in the policy");
        }
        else
        {
            Check.Skip("composite index over a complex collection path (/Orders/[]/Total) is in the policy",
                "the Linux emulator rejects composite indexes over array paths");
        }

        // Azure Cosmos DB needs the composite index to ORDER BY two properties and fails the query without it.
        // The emulator runs the query either way, so this checks the results, not that the index was used.
        capture.Clear();
        var byCityThenName = await db.Customers
            .OrderBy(c => c.ShippingAddress.City)
            .ThenBy(c => c.Name)
            .Select(c => c.Name)
            .ToListAsync();
        Console.WriteLine(capture.LastQuery);
        Check.That(byCityThenName.SequenceEqual(["Dave", "Bob", "Alice", "Carol"]),
            "ORDER BY over the composite index (city, then name) returns the expected order");
    }
}
