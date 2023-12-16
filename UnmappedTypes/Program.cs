using Microsoft.EntityFrameworkCore;
using UnmappedTypes;

Console.WriteLine("Unmapped Types Sample");

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

Console.WriteLine("Querying Data");
var allProducts = db.Database
    .SqlQuery<UnmappedProduct>($"SELECT Name FROM Products")
    .ToList();

allProducts.ForEach(Console.WriteLine);

Console.ReadLine();
