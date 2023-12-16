using CompiledModels;

Console.WriteLine("Compiled Models Sample");

using var db = new ApplicationDbContext();
db.Database.EnsureDeleted();
db.Database.EnsureCreated();

var products = Enumerable
    .Range(1, 10)
    .Select(i => new Product { Name = $"Product {i}", Price = i * 10.00M });

db.Products.AddRange(products);
db.SaveChanges();

var allProducts = db.Products.ToList();
allProducts.ForEach(Console.WriteLine);

Console.ReadLine();