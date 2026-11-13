using System.Text.Json;
using System.Text.Json.Nodes;
using Common;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CosmosDb;

/// <summary>Complex types in Cosmos: nested JSON objects and arrays, queried, inserted and updated.</summary>
public static class ComplexTypesDemo
{
    private static readonly JsonSerializerOptions IndentedJson = new() { WriteIndented = true };

    public static async Task RunAsync()
    {
        Console.WriteLine();
        Console.WriteLine("== Complex types ==");

        var capture = new CosmosCapture();

        await using (var db = new ApplicationDbContext(capture))
        {
            db.Customers.AddRange(SeedData.Customers());
            await db.SaveChangesAsync();
        }

        // A fresh context, so everything below comes back from Cosmos rather than the change tracker.
        await using (var db = new ApplicationDbContext(capture))
        {
            var alice = await db.Customers.SingleAsync(c => c.Id == "alice");
            Console.WriteLine($"Loaded: {alice}");

            // The raw document, read through the Cosmos SDK, shows how complex types are embedded.
            var document = await db.Database.GetCosmosClient()
                .GetContainer(ApplicationDbContext.DatabaseName, ApplicationDbContext.CustomersContainer)
                .ReadItemAsync<JsonObject>("alice", new PartitionKey("AU"));
            Console.WriteLine(document.Resource.ToJsonString(IndentedJson));
            Check.That(document.Resource["ShippingAddress"]?["Location"] is JsonObject,
                "complex type is stored as a nested JSON object");
            Check.That(document.Resource["Orders"] is JsonArray { Count: 2 },
                "complex collection is stored as a JSON array");

            Check.Equal(new Address { Street = "1 George St", City = "Sydney", Postcode = "2000", Location = new Geo(-33.86, 151.21) }, alice.ShippingAddress,
                "nested complex type (Address with a nested Geo) round-trips");
            Check.That(alice.Orders.SequenceEqual([new Order("A-1", 120.50m, "Shipped"), new Order("A-2", 35m, "Pending")]),
                "complex collection (Orders) round-trips in order");

            capture.Clear();
            var inSydney = await db.Customers
                .Where(c => c.ShippingAddress.City == "Sydney")
                .OrderBy(c => c.Name)
                .ToListAsync();
            Console.WriteLine(capture.LastQuery);
            Check.That(inSydney.Select(c => c.Id).SequenceEqual(["alice", "carol"]),
                "filter on a nested complex property returns the expected customers");
            Check.That(capture.LastQuery.Contains("[\"ShippingAddress\"][\"City\"]", StringComparison.Ordinal),
                "Cosmos SQL filters on the nested JSON path");

            capture.Clear();
            var withPendingOrders = await db.Customers
                .Where(c => c.Orders.Any(o => o.Status == "Pending"))
                .OrderBy(c => c.Name)
                .ToListAsync();
            Console.WriteLine(capture.LastQuery);
            Check.That(withPendingOrders.Select(c => c.Id).SequenceEqual(["alice", "bob"]),
                "filter inside a complex collection returns the expected customers");
            Check.That(capture.LastQuery.Contains("EXISTS", StringComparison.Ordinal),
                "Cosmos SQL queries the embedded array with EXISTS");

            // Complex types are values: replace the whole record, or add to the collection, and
            // SaveChanges replaces the document.
            alice.ShippingAddress = alice.ShippingAddress with { Street = "200 George St" };
            alice.Orders.Add(new Order("A-3", 99.99m, "Pending"));
            capture.Clear();
            await db.SaveChangesAsync();
            Check.Equal(1, capture.Of<CosmosItemCommandExecutedEventData>().Count,
                "updating complex values replaces the one document");
        }

        await using (var db = new ApplicationDbContext(capture))
        {
            var alice = await db.Customers.SingleAsync(c => c.Id == "alice");
            Check.Equal("200 George St", alice.ShippingAddress.Street, "updated nested value is persisted");
            Check.Equal(3, alice.Orders.Count, "added complex-collection element is persisted");
        }
    }
}
