using EnhancedJsonColumns;
using Microsoft.EntityFrameworkCore;

Console.WriteLine("Enhanced JSON Columns Sample");

using var db = new ApplicationDbContext();
db.Database.EnsureDeleted();
db.Database.EnsureCreated();

var products = new List<Product>
{
    new()
    {
        Name = "Product 1",
        Colors =
        [
            new Color { Name = "Black", Code = ColorCode.Black, NumInStock = 10 },
            new Color { Name = "White", Code = ColorCode.White, NumInStock = 5 },
            new Color { Name = "Red", Code = ColorCode.Red, NumInStock = 2 },
        ]
    },
    new()
    {
        Name = "Product 2",
        Colors =
        [
            new Color { Name = "Blue", Code = ColorCode.Blue, NumInStock = 10 },
            new Color { Name = "Brown", Code = ColorCode.Brown, NumInStock = 5 },
            new Color { Name = "Green", Code = ColorCode.Green, NumInStock = 2 },
        ]
    },
    new()
    {
        Name = "Product 3",
        Colors =
        [
            new Color { Name = "Black", Code = ColorCode.Black, NumInStock = 10 },
            new Color { Name = "Purple", Code = ColorCode.Purple, NumInStock = 5 },
            new Color { Name = "Yellow", Code = ColorCode.Yellow, NumInStock = 2 },
        ]
    },
};

db.Products.AddRange(products);
db.SaveChanges();

var blackProducts = db.Products
    .AsNoTracking()
    .SelectMany(p => p.Colors.Where(c => c.Code == ColorCode.Black))
    .ToList();

Console.WriteLine("Black Products");
blackProducts.ForEach(Console.WriteLine);

Console.ReadLine();