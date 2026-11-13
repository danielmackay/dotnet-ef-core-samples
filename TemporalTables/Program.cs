using Common;
using Microsoft.EntityFrameworkCore;
using TemporalTables;

Console.WriteLine("Temporal Tables Sample");

var sql = new SqlCapture();
using var db = new ApplicationDbContext(sql);
db.Database.EnsureDeleted();
db.Database.EnsureCreated();

var product = new Product() { Name = "Product 1", Price = 10.00M };

db.Products.Add(product);
db.SaveChanges();
var originalTs = DateTime.UtcNow;

Thread.Sleep(2000);

product.Price = 15.00M;
db.SaveChanges();
var middleTs = DateTime.UtcNow;

Thread.Sleep(2000);

product.Price = 20.00M;
db.SaveChanges();
var latestTs = DateTime.UtcNow;

// Read through a fresh change tracker so the period values come from the database.
db.ChangeTracker.Clear();

Console.WriteLine("Original Product");
product = db.Products.TemporalAsOf(originalTs).First();
PrintTimestamp(originalTs);
Console.WriteLine(product);

Console.WriteLine("Middle Product");
product = db.Products.TemporalAsOf(middleTs).First();
PrintTimestamp(middleTs);
Console.WriteLine(product);

Console.WriteLine("Latest Product");
product = db.Products.TemporalAsOf(latestTs).First();
PrintTimestamp(latestTs);
Console.WriteLine(product);

Console.WriteLine();
Console.WriteLine("Full history, ordered and projected by the CLR period properties");
sql.Clear();
var history = db.Products
    .TemporalAll()
    .OrderBy(p => p.PeriodStart)
    .Select(p => new { p.Price, p.PeriodStart, p.PeriodEnd })
    .ToList();
history.ForEach(h => Console.WriteLine($"Price: {h.Price}, Period: {h.PeriodStart:HH:mm:ss.fff} - {h.PeriodEnd:yyyy-MM-dd HH:mm:ss}"));
Console.WriteLine(sql.Last);

Console.WriteLine();
Check.Equal(3, history.Count, "history has one row per version");
Check.That(history.Select(h => h.Price).SequenceEqual([10.00M, 15.00M, 20.00M]), "history comes back oldest first");
Check.That(history.All(h => h.PeriodStart != default && h.PeriodEnd != default), "period values are populated");
Check.That(history.Zip(history.Skip(1)).All(pair => pair.First.PeriodStart < pair.Second.PeriodStart), "PeriodStart increases from row to row");
Check.That(history.Zip(history.Skip(1)).All(pair => pair.First.PeriodEnd == pair.Second.PeriodStart), "each version ends when the next one starts");
Check.Equal(DateTime.MaxValue.Date, history[^1].PeriodEnd.Date, "current version is open-ended (PeriodEnd = 9999-12-31)");
Check.Equal(history[^1].PeriodStart, product.PeriodStart, "entity query and projection agree on PeriodStart");
Check.That(sql.Last.Contains("FOR SYSTEM_TIME ALL", StringComparison.Ordinal), "SQL queries the temporal history");
Check.That(sql.Last.Contains("ORDER BY [p].[PeriodStart]", StringComparison.Ordinal), "SQL orders by the period column directly");
Check.Summary();

Console.ReadLine();

static void PrintTimestamp(DateTime timestamp) =>
    Console.WriteLine($"Timestamp - {timestamp:HH:mm:ss}");
