using EnhancedBulkUpdateAndDelete;
using Microsoft.EntityFrameworkCore;

Console.WriteLine("Enhanced Bulk Update and Delete Sample");

using var db = new ApplicationDbContext();
db.Database.EnsureDeleted();
db.Database.EnsureCreated();

var products = new List<Product>
{
    new() { Name = "Product 1", Color = new Color { Name = "Black", Code = ColorCode.Black, NumInStock = 10 }, },
    new() { Name = "Product 2", Color = new Color { Name = "Red", Code = ColorCode.Red, NumInStock = 5 }, },
    new() { Name = "Product 3", Color = new Color { Name = "White", Code = ColorCode.White, NumInStock = 2 }, },
};

db.Products.AddRange(products);
db.SaveChanges();

db.Products
    .ExecuteUpdate(u => u.SetProperty(p => p.Color.NumInStock, 0));

var allProducts = db.Products.AsNoTracking().ToList();
Console.WriteLine("All Products");
allProducts.ForEach(Console.WriteLine);

Console.ReadLine();