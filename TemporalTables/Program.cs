using Microsoft.EntityFrameworkCore;
using TemporalTables;

Console.WriteLine("Temporal Tables Sample");

using var db = new ApplicationDbContext();
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

var temporalAll = db.Products.TemporalAll().ToList();
temporalAll.ForEach(Console.WriteLine);

Console.ReadLine();

static void PrintTimestamp(DateTime timestamp) =>
    Console.WriteLine($"Timestamp - {timestamp:HH:mm:ss}");