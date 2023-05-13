using CompiledQueries;

Console.WriteLine("Compiled Queries Sample");

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
Console.WriteLine("Products added");

var product = ProductQueries.GetFirstOrDefault(db, "Product 1");
Console.WriteLine(product);

var allProducts = ProductQueries.GetAll(db);
allProducts.ForEach(Console.WriteLine);

Console.ReadLine();