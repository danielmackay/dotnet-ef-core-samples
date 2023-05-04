using Microsoft.EntityFrameworkCore;

Console.WriteLine("DateOnly TimeOnly Sample");

using var db = new ApplicationDbContext();
await db.Database.EnsureDeletedAsync();
await db.Database.EnsureCreatedAsync();

var date = DateOnly.FromDateTime(DateTime.UtcNow);
var time = TimeOnly.FromDateTime(DateTime.UtcNow);

var products = new List<Product>()
{
    new Product() { Name = "Product 1", DateCreated = date, TimeCreated = time },
    new Product() { Name = "Product 2", DateCreated = date, TimeCreated = time },
    new Product() { Name = "Product 3", DateCreated = date, TimeCreated = time }
};

await db.Products.AddRangeAsync(products);
await db.SaveChangesAsync();

var allProducts = await db.Products.ToListAsync();
allProducts.ForEach(p => Console.WriteLine($"Product {p.Name}"));

Console.ReadLine();