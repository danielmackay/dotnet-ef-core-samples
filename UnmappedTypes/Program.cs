using JsonColumns;
using Microsoft.EntityFrameworkCore;

Console.WriteLine("Unmapped Types Sample");

using var db = new ApplicationDbContext();
await db.Database.EnsureDeletedAsync();
await db.Database.EnsureCreatedAsync();

var products = new List<Product>()
{
    new Product() { Name = "Product 1" },
    new Product() { Name = "Product 2" },
    new Product() { Name = "Product 3" },
    new Product() { Name = "Product 4" },
    new Product() { Name = "Product 5" },
    new Product() { Name = "Product 6", IsDeleted = true },
    new Product() { Name = "Product 7", IsDeleted = true },
    new Product() { Name = "Product 8", IsDeleted = true },
    new Product() { Name = "Product 9" , IsDeleted = true},
    new Product() { Name = "Product 10" , IsDeleted = true}
};

await db.Products.AddRangeAsync(products);
await db.SaveChangesAsync();

Console.WriteLine("Querying Data");
var allProducts = await db.Database
    .SqlQuery<UnmappedProduct>($"SELECT Name FROM Products")
    .ToListAsync();

allProducts.ForEach(p => Console.WriteLine(p.Name));

Console.ReadLine();
