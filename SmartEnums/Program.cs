using SmartEnums;

Console.WriteLine("Owned Entities Sample");

using var db = new ApplicationDbContext();
db.Database.EnsureDeleted();
db.Database.EnsureCreated();

var products = new List<Product>
{   new()
    {
        Name = "Laptop",
        Type = ProductType.Physical,
        Categories =
        [
            ProductCategory.Electronics,
            ProductCategory.HomeGoods
        ]
    },
    new()
    {
        Name = "E-book",
        Type = ProductType.Digital,
        Categories =
        [
            ProductCategory.Books,
            ProductCategory.Electronics
        ]
    },
};

db.Products.AddRange(products);
db.SaveChanges();

var allContacts = db.Products.ToList();
allContacts.ForEach(Console.WriteLine);

var books = db.Products
    .Where(p => p.Categories.Contains(ProductCategory.Books))
    .ToList();
books.ForEach(Console.WriteLine);

Console.ReadLine();