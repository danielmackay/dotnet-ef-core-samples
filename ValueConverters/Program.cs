Console.WriteLine("Value Converters Sample");

using var db = new ApplicationDbContext();
db.Database.EnsureDeleted();
db.Database.EnsureCreated();

var products = new List<Product>()
{
    new()
    {
        Id = new ProductId(Guid.NewGuid()),
        Name = "Product 1",
        Price = 10.00M,
    },
    new()
    {
        Id = new ProductId(Guid.NewGuid()),
        Name = "Product 2",
        Price = 20.00M,
    },
    new()
    {
        Id = new ProductId(Guid.NewGuid()),
        Name = "Product 3",
        Price = 30.00M,
    },
};

db.Products.AddRange(products);
db.SaveChanges();

var allProducts = db.Products.ToList();
allProducts.ForEach(Console.WriteLine);

Console.ReadLine();