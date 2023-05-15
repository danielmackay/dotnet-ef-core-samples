Console.WriteLine("Buffering and Streaming Sample");

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

using var db = new ApplicationDbContext();
db.Database.EnsureDeleted();
db.Database.EnsureCreated();

var products = Enumerable
    .Range(1, 100)
    .Select(i => new Product { Name = $"Product {i}", Price = i * 10.00M });

db.Products.AddRange(products);
db.SaveChanges();

app.MapGet("/api/buffered", (ILogger logger) =>
{
    // All results fetched from DB and buffered
    var bufferedProducts = db.Products.ToList();
    bufferedProducts.ForEach(p => logger.LogInformation(p.ToString()));
});

app.MapGet("/api/streamed", (ILogger logger) =>
{
    // Results streamed from the DB
    foreach (var p in db.Products)
        logger.LogInformation(p.ToString());
});

app.MapGet("/api/streamed-via-api", () =>
{
    // Returning an IEnumerable or IQueryable will stream these via the API
    return db.Products;
});

app.Run();
