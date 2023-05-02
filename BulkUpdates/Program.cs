using Microsoft.EntityFrameworkCore;

Console.WriteLine("Bulk Updates Sample");

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

var allProducts = await db.Products.ToListAsync();
Console.WriteLine($"Visible products: {allProducts.Count}");

await db.Products
    .Where(p => p.IsDeleted)
    .ExecuteUpdateAsync(u => u.SetProperty(p => p.Name, p => p.Name + "(DELETED)"));

var allProducts2 = await db.Products
    .AsNoTracking()
    .ToListAsync();

allProducts2.ForEach(p => Console.WriteLine(p.Name));

Console.ReadLine();