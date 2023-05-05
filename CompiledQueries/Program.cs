using CompiledQueries;

Console.WriteLine("Compiled Queries Sample");

using var db = new ApplicationDbContext();
await db.Database.EnsureDeletedAsync();
await db.Database.EnsureCreatedAsync();

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

await db.Products.AddRangeAsync(products);
await db.SaveChangesAsync();
Console.WriteLine("Products added");

var product = ProductQueries.GetFirstOrDefault(db, "Product 1");
Console.WriteLine($"Product {product?.Name}");

var allProducts = ProductQueries.GetAll(db);
allProducts.ForEach(p => Console.WriteLine($"Product {p.Name}"));

Console.ReadLine();