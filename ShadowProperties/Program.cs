Console.WriteLine("Shadow Properties Sample");

using var db = new ApplicationDbContext();
db.Database.EnsureDeleted();
db.Database.EnsureCreated();

var product = new Product
{
    Name = "Product 1",
    Price = 10.00M,
    Customer = new Customer
    {
        Name = "Customer 1"
    }
};

db.Products.Add(product);
db.SaveChanges();

product = db.Products.First();
PrintProduct(product);

Console.ReadLine();

static void PrintProduct(Product product) =>
    Console.WriteLine($"Product - ID:{product.Id}, Name:{product.Name}, Price:{product.Price}");