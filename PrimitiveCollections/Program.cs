using PrimitiveCollections;

Console.WriteLine("Primitive Collections Sample");

using var db = new ApplicationDbContext();
db.Database.EnsureDeleted();
db.Database.EnsureCreated();

var products = new List<Product>()
{
    new() { Name = "Product 1", Colors = [Color.Black, Color.White, Color.Red]},
    new() { Name = "Product 2", Colors = [Color.Blue, Color.Brown, Color.Green]},
    new() { Name = "Product 3", Colors = [Color.Orange, Color.Purple, Color.Yellow]},
};

db.Products.AddRange(products);
db.SaveChanges();

var allProducts = db.Products.ToList();
Console.WriteLine("All Products");
allProducts.ForEach(Console.WriteLine);

Console.ReadLine();