using Microsoft.EntityFrameworkCore;

Console.WriteLine("Tags Sample");

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

var visibleProducts = db.Products
    .Where(p => !p.IsDeleted)
    .TagWith("Visible Products")
    .ToList();

Console.WriteLine($"Visible products");
visibleProducts.ForEach(Console.WriteLine);

var allProducts = db.Products
    .TagWith("All Products")
    .ToList();

Console.WriteLine($"All products (including deleted)");
allProducts.ForEach(Console.WriteLine);

Console.ReadLine();