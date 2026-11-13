using Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CosmosDb;

/// <summary>
/// SaveChanges groups writes into Cosmos transactional batches (one per container and partition), and
/// <see cref="AutoTransactionBehavior"/> decides how much atomicity you get.
/// </summary>
public static class TransactionalBatchDemo
{
    // Own partitions, so the documents written here don't disturb the other demos' checks.
    private const string PartitionA = "TX-A";
    private const string PartitionB = "TX-B";

    public static async Task RunAsync()
    {
        Console.WriteLine();
        Console.WriteLine("== Transactional batches ==");

        var capture = new CosmosCapture();

        // Auto (the default): several writes to one partition go out as a single transactional batch.
        await using (var db = new ApplicationDbContext(capture))
        {
            db.Customers.AddRange(NewCustomer("tx-1", PartitionA), NewCustomer("tx-2", PartitionA));
            capture.Clear();
            await db.SaveChangesAsync();

            var batches = capture.Of<CosmosTransactionalBatchExecutedEventData>();
            Console.WriteLine($"Auto: {batches.Count} batch(es) for 2 documents in one partition");
            Check.Equal(1, batches.Count, "Auto: two same-partition inserts go out as one transactional batch");
        }

        // Auto is atomic within a batch: a conflict on one document rolls back the whole batch.
        await using (var db = new ApplicationDbContext(capture))
        {
            // tx-1 already exists, so its insert conflicts.
            db.Customers.AddRange(NewCustomer("tx-3", PartitionA), NewCustomer("tx-1", PartitionA));
            Check.That(await SaveFailsAsync<DbUpdateException>(db), "Auto: the batch with a conflicting document fails");
        }
        Check.That(!await ExistsAsync("tx-3", PartitionA, capture),
            "Auto: the other document in the failed batch was rolled back (not written)");

        // Never: each write is its own request, so the ones that succeed stay written.
        await using (var db = new ApplicationDbContext(capture))
        {
            db.Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;
            db.Customers.AddRange(NewCustomer("tx-4", PartitionA), NewCustomer("tx-1", PartitionA));
            capture.Clear();
            Check.That(await SaveFailsAsync<DbUpdateException>(db), "Never: the save with a conflicting document fails");
            Check.Equal(0, capture.Of<CosmosTransactionalBatchExecutedEventData>().Count,
                "Never: no transactional batch is used");
        }
        Check.That(await ExistsAsync("tx-4", PartitionA, capture),
            "Never: the non-conflicting document is kept (no atomicity)");

        // Always: EF refuses a save it can't make atomic. Two partitions can't share one batch.
        await using (var db = new ApplicationDbContext(capture))
        {
            db.Database.AutoTransactionBehavior = AutoTransactionBehavior.Always;
            db.Customers.AddRange(NewCustomer("tx-5", PartitionA), NewCustomer("tx-6", PartitionB));
            capture.Clear();
            Check.That(await SaveFailsAsync<InvalidOperationException>(db),
                "Always: a save spanning two partitions throws InvalidOperationException");
            Check.Equal(0, capture.Of<CosmosItemCommandExecutedEventData>().Count
                           + capture.Of<CosmosTransactionalBatchExecutedEventData>().Count,
                "Always: nothing is sent to Cosmos");
        }
        Check.That(!await ExistsAsync("tx-5", PartitionA, capture) && !await ExistsAsync("tx-6", PartitionB, capture),
            "Always: neither document was written");

        // Always with a single partition is fine: one batch, all or nothing.
        await using (var db = new ApplicationDbContext(capture))
        {
            db.Database.AutoTransactionBehavior = AutoTransactionBehavior.Always;
            db.Customers.AddRange(NewCustomer("tx-5", PartitionA), NewCustomer("tx-6", PartitionA));
            capture.Clear();
            await db.SaveChangesAsync();
            Check.Equal(1, capture.Of<CosmosTransactionalBatchExecutedEventData>().Count,
                "Always: a single-partition save goes out as one transactional batch");
        }
    }

    private static async Task<bool> SaveFailsAsync<TException>(DbContext db) where TException : Exception
    {
        try
        {
            await db.SaveChangesAsync();
            return false;
        }
        catch (TException ex)
        {
            Console.WriteLine($"  {typeof(TException).Name}: {(ex.InnerException?.Message ?? ex.Message).FirstLine()}");
            return true;
        }
    }

    private static async Task<bool> ExistsAsync(string id, string region, CosmosCapture capture)
    {
        await using var db = new ApplicationDbContext(capture);
        return await db.Customers.FindAsync(id, region) is not null;
    }

    private static Customer NewCustomer(string id, string region) => new()
    {
        Id = id,
        Region = region,
        Name = id,
        ShippingAddress = new Address { Street = "1 Batch St", City = "Brisbane", Postcode = "4000", Location = new Geo(-27.47, 153.03) },
    };
}
