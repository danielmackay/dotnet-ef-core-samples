using BulkUpdates;
using Microsoft.EntityFrameworkCore;

Console.WriteLine("Bulk Updates Sample");

using var db = new ApplicationDbContext();
db.Database.EnsureDeleted();
db.Database.EnsureCreated();

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

db.Products.AddRange(products);
db.SaveChanges();

var allProducts = db.Products.ToList();
Console.WriteLine("All Products");
allProducts.ForEach(Console.WriteLine);

db.Products
    .Where(p => p.IsDeleted)
    .ExecuteUpdate(u => u.SetProperty(p => p.Name, p => p.Name + "(DELETED)"));

var allProducts2 = db.Products
    .AsNoTracking()
    .ToList();

Console.WriteLine("All Products 2");
allProducts2.ForEach(Console.WriteLine);

Console.ReadLine();