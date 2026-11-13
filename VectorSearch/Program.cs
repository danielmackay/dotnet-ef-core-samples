using Common;
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlTypes;
using Microsoft.EntityFrameworkCore;
using VectorSearch;

Console.WriteLine("Vector Search Sample");

var sql = new SqlCapture();
using var db = new ApplicationDbContext(sql);
db.Database.EnsureDeleted();

// On SQL Server 2025 a table becomes read-only once it has a vector index (Azure SQL's newer
// vector indexes lift that), so the sample creates the table, inserts the rows, and only then
// applies the migration that adds the index.
db.Database.Migrate("InitialCreate");
db.Articles.AddRange(SampleArticles.Create());
db.SaveChanges();
db.ChangeTracker.Clear();
db.Database.Migrate();

Console.WriteLine();
Console.WriteLine("1. The vector index created by the migration");
var vectorIndexes = db.Database
    .SqlQuery<string>($"""
        SELECT i.name AS [Value]
        FROM sys.vector_indexes v
        JOIN sys.indexes i ON i.object_id = v.object_id AND i.index_id = v.index_id
        WHERE v.object_id = OBJECT_ID('Articles')
        """)
    .ToList();
Console.WriteLine($"Vector indexes on Articles: {string.Join(", ", vectorIndexes)}");
Check.That(vectorIndexes.SequenceEqual(["IX_Articles_Embedding"]), "sys.vector_indexes lists IX_Articles_Embedding");

// The nearest articles to the query, worked out by hand from the embeddings in SampleArticles.
string[] expectedTop3 = ["Querying JSON in SQL Server", "Tuning SQL Server indexes", "Vector databases explained"];
var query = SampleArticles.DatabaseQuery;

Console.WriteLine();
Console.WriteLine("2. Approximate search: VectorSearch(...).Take(3).WithApproximate() uses the vector index");
var approximate = db.Articles
    .VectorSearch(a => a.Embedding, query, "cosine")
    .OrderBy(r => r.Distance)
    .Take(3)
    .WithApproximate();
var approximateSql = approximate.ToQueryString();
Console.WriteLine(approximateSql);
Check.That(approximateSql.Contains("VECTOR_SEARCH(", StringComparison.Ordinal), "approximate SQL uses VECTOR_SEARCH");
Check.That(approximateSql.Contains("WITH APPROXIMATE", StringComparison.Ordinal), "approximate SQL uses TOP(...) WITH APPROXIMATE");

var approximateResults = RunVectorSearch(approximate, "approximate VectorSearch results");
if (approximateResults is not null)
{
    Print(approximateResults);
    Check.That(approximateResults.Select(r => r.Value.Title).SequenceEqual(expectedTop3), "approximate search returns the 3 database articles, nearest first");
    Check.Ascending(approximateResults.Select(r => r.Distance), "approximate search distances are in ascending order");
}

Console.WriteLine();
Console.WriteLine("3. Exact search: the same query without WithApproximate() compares the query with every row");
var exact = db.Articles
    .VectorSearch(a => a.Embedding, query, "cosine")
    .OrderBy(r => r.Distance)
    .Take(3);
var exactSql = exact.ToQueryString();
Console.WriteLine(exactSql);
Check.That(exactSql.Contains("VECTOR_SEARCH(", StringComparison.Ordinal) && !exactSql.Contains("APPROXIMATE", StringComparison.Ordinal),
    "exact SQL uses VECTOR_SEARCH without WITH APPROXIMATE");

var exactResults = RunVectorSearch(exact, "exact VectorSearch results");
if (exactResults is not null)
{
    Print(exactResults);
    Check.That(exactResults.Select(r => r.Value.Title).SequenceEqual(expectedTop3), "exact VectorSearch returns the 3 database articles, nearest first");
}

// VECTOR_DISTANCE (EF Core 10) is the other way to run an exact search, and it works on every
// SQL Server 2025 build, so it's the exact search this sample can always execute.
Console.WriteLine("Exact search with EF.Functions.VectorDistance:");
sql.Clear();
var nearest = db.Articles
    .Select(a => new { a.Title, Distance = EF.Functions.VectorDistance("cosine", a.Embedding, query) })
    .OrderBy(a => a.Distance)
    .Take(3)
    .ToList();
Console.WriteLine(sql.Last);
foreach (var item in nearest)
    Console.WriteLine($"  {item.Distance:F4}  {item.Title}");
Check.That(nearest.Select(a => a.Title).SequenceEqual(expectedTop3), "exact search returns the 3 database articles, nearest first");
Check.Ascending(nearest.Select(a => a.Distance), "exact search distances are in ascending order");

Console.WriteLine();
Console.WriteLine("4. EF Core 11 no longer reads vector columns when it loads an entity");
sql.Clear();
var loaded = db.Articles.Single(a => a.Title == "Querying JSON in SQL Server");
Console.WriteLine(sql.Last);
Console.WriteLine($"Loaded entity: Embedding.Length = {loaded.Embedding.Length}");
Check.That(!sql.Last.Contains("[Embedding]", StringComparison.Ordinal), "loading an entity doesn't select the Embedding column");
// SqlVector<float> is a struct, so an unloaded vector isn't a C# null: EF leaves the property at
// default(SqlVector<float>), which has no values. Don't test IsNull: in SqlClient 7.1 it's false
// for default(SqlVector<float>).
Check.That(loaded.Embedding.Equals(default(SqlVector<float>)) && loaded.Embedding.Length == 0,
    "a loaded entity's Embedding is empty (left at its default)");

sql.Clear();
var projected = db.Articles
    .Where(a => a.Title == "Querying JSON in SQL Server")
    .Select(a => new { a.Title, a.Embedding })
    .Single();
Console.WriteLine(sql.Last);
var projectedValues = projected.Embedding.Memory.ToArray();
Console.WriteLine($"Projected Embedding: [{string.Join(", ", projectedValues)}]");
Check.That(projectedValues.SequenceEqual([0.90f, 0.00f, 0.00f, 0.00f]), "an explicit projection returns the stored vector");

Check.Summary();

Console.ReadLine();

// EF Core 11 emits the latest VECTOR_SEARCH syntax (TOP(n) WITH APPROXIMATE), which boxed SQL Server
// 2025 rejects (see Common/VectorSearchSyntax). The SQL is still checked above, so the sample skips
// only the execution there. Any other error fails the sample.
static List<VectorSearchResult<Article>>? RunVectorSearch(IQueryable<VectorSearchResult<Article>> search, string description)
{
    try
    {
        return search.ToList();
    }
    catch (SqlException ex) when (VectorSearchSyntax.IsUnsupported(ex))
    {
        Check.Skip(description, $"this SQL Server doesn't support EF Core 11's VECTOR_SEARCH syntax yet: {ex.Message.FirstLine()}");
        return null;
    }
}

static void Print(IEnumerable<VectorSearchResult<Article>> results)
{
    foreach (var (article, distance) in results)
        Console.WriteLine($"  {distance:F4}  {article.Title}");
}
