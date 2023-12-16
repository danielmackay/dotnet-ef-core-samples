using DateOnlyTimeOnly;

Console.WriteLine("DateOnly TimeOnly Sample");

using var db = new ApplicationDbContext();
db.Database.EnsureDeleted();
db.Database.EnsureCreated();

var date = DateOnly.FromDateTime(DateTime.UtcNow);
var time = TimeOnly.FromDateTime(DateTime.UtcNow);

var products = new List<Product>()
{
    new Product() { Name = "Product 1", DateCreated = date, TimeCreated = time },
    new Product() { Name = "Product 2", DateCreated = date, TimeCreated = time },
    new Product() { Name = "Product 3", DateCreated = date, TimeCreated = time }
};

db.Products.AddRange(products);
db.SaveChanges();

var allProducts = db.Products.ToList();
allProducts.ForEach(Console.WriteLine);

Console.ReadLine();