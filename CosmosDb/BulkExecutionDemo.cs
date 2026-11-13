using System.Diagnostics;
using Common;
using Microsoft.EntityFrameworkCore;

namespace CosmosDb;

/// <summary>
/// Bulk execution hands SaveChanges' writes to the Cosmos SDK's bulk mode, which packs many operations into
/// fewer requests for throughput. It trades away transactional batches, so it needs AutoTransactionBehavior.Never.
/// </summary>
public static class BulkExecutionDemo
{
    // Own partition, so these documents don't show up in the other demos' queries.
    private const string Region = "BULK";
    private const int DocumentCount = 500;

    public static async Task RunAsync()
    {
        Console.WriteLine();
        Console.WriteLine("== Bulk execution ==");

        var capture = new CosmosCapture();

        // With the default AutoTransactionBehavior.Auto, EF would use transactional batches and skip bulk mode,
        // so it raises the BulkExecutionWithTransactionalBatch warning, which is an error by default.
        await using (var db = new ApplicationDbContext(capture, cosmos => cosmos.BulkExecutionAllowed()))
        {
            db.Customers.Add(NewCustomer(0));
            try
            {
                await db.SaveChangesAsync();
                Check.That(false, "bulk execution with AutoTransactionBehavior.Auto throws");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"  {ex.Message.Split('\n')[0].Trim()}");
                Check.That(ex.Message.Contains(nameof(AutoTransactionBehavior.Never), StringComparison.Ordinal),
                    "bulk execution with AutoTransactionBehavior.Auto throws, pointing at AutoTransactionBehavior.Never");
            }
        }

        await using (var db = new ApplicationDbContext(capture, cosmos => cosmos.BulkExecutionAllowed()))
        {
            db.Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;
            db.Customers.AddRange(Enumerable.Range(1, DocumentCount).Select(NewCustomer));

            var stopwatch = Stopwatch.StartNew();
            await db.SaveChangesAsync();
            Console.WriteLine($"Inserted {DocumentCount} documents with bulk execution in {stopwatch.ElapsedMilliseconds} ms");
        }

        await using (var db = new ApplicationDbContext(capture))
        {
            var count = await db.Customers.CountAsync(c => c.Region == Region);
            Check.Equal(DocumentCount, count, $"bulk execution inserted all {DocumentCount} documents");
        }
    }

    private static Customer NewCustomer(int i) => new()
    {
        Id = $"bulk-{i:D4}",
        Region = Region,
        Name = $"Bulk customer {i}",
        ShippingAddress = new Address { Street = $"{i} Bulk Rd", City = "Perth", Postcode = "6000", Location = new Geo(-31.95, 115.86) },
    };
}
