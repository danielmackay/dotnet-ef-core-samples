using Microsoft.EntityFrameworkCore;

Console.WriteLine("Bulk Deletes Sample");

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
allProducts.ForEach(Console.WriteLine);

db.Products
    .Where(p => p.IsDeleted)
    .ExecuteDelete();

var allProducts2 = db.Products
    .AsNoTracking()
    .ToList();

allProducts2.ForEach(Console.WriteLine);

Console.ReadLine();