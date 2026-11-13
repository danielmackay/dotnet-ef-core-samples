using Common;
using JsonFunctions;
using Microsoft.EntityFrameworkCore;

Console.WriteLine("JSON Functions Sample");

var sql = new SqlCapture();
using var db = new ApplicationDbContext(sql);
db.Database.EnsureDeleted();
db.Database.EnsureCreated();

db.Products.AddRange(
    new Product { Name = "Keyboard", JsonData = """{"Rating": 8, "OptionalInt": 5}""", Tags = ["ef-core", "hardware"] },
    new Product { Name = "Mouse", JsonData = """{"Rating": 6}""", Tags = ["hardware"] },
    new Product { Name = "Monitor", JsonData = """{"Rating": 8, "OptionalInt": null}""", Tags = ["display"] },
    new Product { Name = "Book", JsonData = """{"Rating": 9}""", Tags = ["ef-core", "reading"] });
db.SaveChanges();
db.ChangeTracker.Clear();

Console.WriteLine();
Console.WriteLine("1. EF.Functions.JsonPathExists: products whose JSON has an OptionalInt key");
sql.Clear();
var withOptionalInt = db.Products
    .Where(p => EF.Functions.JsonPathExists(p.JsonData, "$.OptionalInt"))
    .OrderBy(p => p.Id)
    .Select(p => p.Name)
    .ToList();
Console.WriteLine(sql.Last);
Console.WriteLine($"Rows: {string.Join(", ", withOptionalInt)}");
Check.That(withOptionalInt.SequenceEqual(["Keyboard", "Monitor"]), "JsonPathExists returns Keyboard and Monitor (a JSON null still counts as present)");
Check.That(sql.Last.Contains("JSON_PATH_EXISTS", StringComparison.Ordinal), "SQL uses JSON_PATH_EXISTS");

Console.WriteLine();
Console.WriteLine("2. EF.Functions.JsonContains: products with a Rating of 8");
sql.Clear();
var ratedEight = db.Products
    .Where(p => EF.Functions.JsonContains(p.JsonData, 8, "$.Rating") == 1)
    .OrderBy(p => p.Id)
    .Select(p => p.Name)
    .ToList();
Console.WriteLine(sql.Last);
Console.WriteLine($"Rows: {string.Join(", ", ratedEight)}");
Check.That(ratedEight.SequenceEqual(["Keyboard", "Monitor"]), "JsonContains returns Keyboard and Monitor");
Check.That(sql.Last.Contains("JSON_CONTAINS", StringComparison.Ordinal), "SQL uses JSON_CONTAINS");

Console.WriteLine();
Console.WriteLine("3. Contains over a primitive collection at compatibility 160 and 170");

var sql160 = new SqlCapture();
using var db160 = new ApplicationDbContext(sql160, compatibilityLevel: 160);
var tagged160 = TaggedEfCore(db160);
Console.WriteLine("Compatibility 160:");
Console.WriteLine(sql160.Last);
Console.WriteLine($"Rows: {string.Join(", ", tagged160)}");

var sql170 = new SqlCapture();
using var db170 = new ApplicationDbContext(sql170, compatibilityLevel: 170);
var tagged170 = TaggedEfCore(db170);
Console.WriteLine("Compatibility 170:");
Console.WriteLine(sql170.Last);
Console.WriteLine($"Rows: {string.Join(", ", tagged170)}");

Check.That(sql160.Last.Contains("OPENJSON", StringComparison.Ordinal), "compatibility 160 SQL uses OPENJSON");
Check.That(sql170.Last.Contains("JSON_CONTAINS", StringComparison.Ordinal), "compatibility 170 SQL uses JSON_CONTAINS");
Check.That(tagged160.SequenceEqual(["Keyboard", "Book"]), "compatibility 160 returns Keyboard and Book");
Check.That(tagged160.SequenceEqual(tagged170), "both compatibility levels return the same rows");

Check.Summary();

Console.ReadLine();

// The tag is an inline constant on purpose: in EF Core 11.0.0 a C# variable (a SQL parameter)
// still translates to OPENJSON at compatibility 170. Only a constant becomes JSON_CONTAINS.
static List<string> TaggedEfCore(ApplicationDbContext db) =>
    db.Products
        .Where(p => p.Tags.Contains("ef-core"))
        .OrderBy(p => p.Id)
        .Select(p => p.Name)
        .ToList();
