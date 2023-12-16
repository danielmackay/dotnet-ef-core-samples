using KeysetPagination;

Console.WriteLine("Keyset Pagination Sample");

using var db = new ApplicationDbContext();
db.Database.EnsureDeleted();
db.Database.EnsureCreated();

var products = Enumerable
    .Range(1, 100)
    .Select(i => new Product { Name = $"Product {i}", Price = i * 10.00M });

db.Products.AddRange(products);
db.SaveChanges();

const int pageSize = 10;
var currentPage = 0;

Console.WriteLine("Offset-based Pagination");
bool hasItems = false;
do
{
    var productPage = db.Products
        .OrderBy(p => p.Id)
        .Skip(pageSize * currentPage)
        .Take(pageSize)
        .ToList();

    productPage.ForEach(Console.WriteLine);
    hasItems = productPage.Count > 0;
    currentPage++;

} while (hasItems);

Console.WriteLine("Keyset-based Pagination");
hasItems = false;
int lastId = 0;
do
{
    var productPage = db.Products
        .OrderBy(p => p.Id)
        .Where(p => p.Id > lastId)
        .Take(pageSize)
        .ToList();

    productPage.ForEach(Console.WriteLine);
    lastId = productPage.LastOrDefault()?.Id ?? 0;
    hasItems = productPage.Count > 0;

} while (hasItems && lastId != 0);

Console.ReadLine();