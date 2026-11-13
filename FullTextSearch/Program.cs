using Common;
using FullTextSearch;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

Console.WriteLine("Full-Text Search Sample");

var sql = new SqlCapture();
await using var db = new ApplicationDbContext(sql);

int? fullTextInstalled;

// Asked of master because the sample database doesn't exist yet.
await using (var master = new SqlConnection(DbConnectionFactory.Create("master")))
{
    await master.OpenAsync();
    await using var command = new SqlCommand("SELECT CAST(FULLTEXTSERVICEPROPERTY('IsFullTextInstalled') AS int)", master);
    fullTextInstalled = await command.ExecuteScalarAsync() is int installed ? installed : null;
}

if (!Check.Equal(1, fullTextInstalled, "SQL Server has Full-Text Search installed"))
{
    Console.WriteLine("Start the AppHost: its SQL Server image adds the full-text package (see readme).");
    return;
}

Console.WriteLine();
Console.WriteLine("1. The migration creates the full-text catalog and index");
var script = db.GetService<IMigrator>().GenerateScript();
Console.WriteLine(script);
// The catalog line is hand-added to the migration because of EF Core 11.0.0 bugs (see readme), so this
// only confirms the migration still has it. EF generates the index line from the model.
Check.That(script.Contains("CREATE FULLTEXT CATALOG [ftCatalog]", StringComparison.Ordinal),
    "migration creates the ftCatalog full-text catalog (hand-added to the migration because of EF bugs)");
var fullTextIndexLine = script.Split('\n').FirstOrDefault(line => line.Contains("CREATE FULLTEXT INDEX", StringComparison.Ordinal)) ?? string.Empty;
Check.That(fullTextIndexLine.Contains("ON [Articles]", StringComparison.Ordinal), "migration creates a full-text index on Articles");
Check.That(fullTextIndexLine.Contains("[Title]", StringComparison.Ordinal) && fullTextIndexLine.Contains("[Contents]", StringComparison.Ordinal),
    "the full-text index covers Title and Contents");
Check.That(fullTextIndexLine.Contains("KEY INDEX [PK_Articles]", StringComparison.Ordinal), "the full-text index is keyed by PK_Articles");
Check.That(fullTextIndexLine.Contains("ON [ftCatalog]", StringComparison.Ordinal), "the full-text index goes in ftCatalog (UseCatalog)");

await db.Database.EnsureDeletedAsync();
await db.Database.MigrateAsync();

var catalogs = await db.Database
    .SqlQueryRaw<string>("SELECT name AS [Value] FROM sys.fulltext_catalogs")
    .ToListAsync();
Check.That(catalogs.Contains(ApplicationDbContext.CatalogName), "ftCatalog exists in sys.fulltext_catalogs");

var indexedColumns = await db.Database
    .SqlQueryRaw<string>("""
        SELECT c.name AS [Value]
        FROM sys.fulltext_indexes i
        JOIN sys.fulltext_catalogs fc ON fc.fulltext_catalog_id = i.fulltext_catalog_id
        JOIN sys.fulltext_index_columns ic ON ic.object_id = i.object_id
        JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
        WHERE i.object_id = OBJECT_ID('Articles') AND fc.name = 'ftCatalog'
        """)
    .ToListAsync();
Check.That(indexedColumns.Order().SequenceEqual(["Contents", "Title"]), "Articles has a full-text index on Contents and Title in ftCatalog");

Console.WriteLine();
Console.WriteLine("2. Seed articles and wait for the full-text index to populate");
db.Articles.AddRange(SampleArticles.Create());
await db.SaveChangesAsync();
var articleCount = await db.Articles.CountAsync();

// The full-text index is populated in the background after rows change, so querying straight
// away can miss rows. Poll until the catalog is idle and the index holds every row.
var populated = await WaitForPopulation(db, articleCount);
Check.That(populated, $"full-text index populated with all {articleCount} articles");

Console.WriteLine();
Console.WriteLine("3. FreeTextTable: articles about 'growing vegetables', by rank");
sql.Clear();
var freeText = await db.Articles
    .Join(
        db.Articles.FreeTextTable<Article, int>("growing vegetables"),
        a => a.Id,
        f => f.Key,
        (a, f) => new { a.Title, f.Rank })
    .OrderByDescending(x => x.Rank)
    .ToListAsync();
Console.WriteLine(sql.Last);
freeText.ForEach(x => Console.WriteLine($"  {x.Rank,4}  {x.Title}"));
Check.That(sql.Last.Contains("FREETEXTTABLE(", StringComparison.Ordinal), "SQL uses FREETEXTTABLE");
Check.Equal(2, freeText.Count, "FREETEXTTABLE matches both articles that mention vegetables");
Check.Equal("Growing vegetables at home", freeText.FirstOrDefault()?.Title, "top FREETEXTTABLE match (the article about growing vegetables, not the passing mention)");
Check.Descending(freeText.Select(x => x.Rank), "FREETEXTTABLE results are in descending rank order");

Console.WriteLine();
Console.WriteLine("4. ContainsTable: carrots (weighted high) or fruit (weighted low) in Contents, by rank");
sql.Clear();
var contains = await db.Articles
    .ContainsTable<Article, int>("ISABOUT(carrots WEIGHT(0.9), fruit WEIGHT(0.1))", a => a.Contents)
    .Join(db.Articles, c => c.Key, a => a.Id, (c, a) => new { a.Title, c.Rank })
    .OrderByDescending(x => x.Rank)
    .ToListAsync();
Console.WriteLine(sql.Last);
contains.ForEach(x => Console.WriteLine($"  {x.Rank,4}  {x.Title}"));
Check.That(sql.Last.Contains("CONTAINSTABLE([Articles], [Contents],", StringComparison.Ordinal), "SQL uses CONTAINSTABLE on the Contents column");
Check.Equal("Winter soup", contains.FirstOrDefault()?.Title, "top CONTAINSTABLE match (carrots outweighs fruit)");
Check.Equal(3, contains.Count, "CONTAINSTABLE matches the soup and both fruit articles");
Check.Descending(contains.Select(x => x.Rank), "CONTAINSTABLE results are in descending rank order");

Console.WriteLine();
Console.WriteLine("5. Hybrid search as the EF Core docs write it: FreeTextTable + VectorSearch(...).WithApproximate(), merged with FullJoin");
var vectorSearchHybrid = HybridSearch.Fuse(HybridSearch.FullTextHits(db), HybridSearch.VectorSearchHits(db));
var vectorSearchHybridSql = vectorSearchHybrid.ToQueryString();
Console.WriteLine(vectorSearchHybridSql);
Check.That(vectorSearchHybridSql.Contains("FREETEXTTABLE(", StringComparison.Ordinal), "VectorSearch hybrid SQL uses FREETEXTTABLE");
Check.That(vectorSearchHybridSql.Contains("VECTOR_SEARCH(", StringComparison.Ordinal) && vectorSearchHybridSql.Contains("WITH APPROXIMATE", StringComparison.Ordinal),
    "VectorSearch hybrid SQL uses VECTOR_SEARCH with TOP(...) WITH APPROXIMATE");
Check.That(vectorSearchHybridSql.Contains("FULL JOIN", StringComparison.Ordinal), "VectorSearch hybrid SQL merges the two searches with FULL JOIN");

// EF Core 11 emits the latest VECTOR_SEARCH syntax (TOP(n) WITH APPROXIMATE), which boxed SQL Server
// 2025 rejects (see Common/VectorSearchSyntax). The SQL is checked above, so only the execution is
// skipped there. Any other error fails the sample.
try
{
    var results = await vectorSearchHybrid.ToListAsync();
    results.ForEach(Console.WriteLine);
    CheckHybridResults(results, "VectorSearch hybrid");
}
catch (SqlException ex) when (VectorSearchSyntax.IsUnsupported(ex))
{
    Check.Skip("VectorSearch hybrid results",
        $"this SQL Server doesn't support EF Core 11's VECTOR_SEARCH syntax yet: {ex.Message.FirstLine()} Section 6 runs the same hybrid search with VECTOR_DISTANCE");
}

Console.WriteLine();
Console.WriteLine("6. Hybrid search that runs on SQL Server 2025: FreeTextTable + VectorDistance, merged with FullJoin");
sql.Clear();
var hybrid = await HybridSearch.Fuse(HybridSearch.FullTextHits(db), HybridSearch.VectorDistanceHits(db)).ToListAsync();
Console.WriteLine(sql.Last);
hybrid.ForEach(Console.WriteLine);
Check.Equal(1, sql.Commands.Count, "hybrid search runs as one SQL statement");
Check.That(sql.Last.Contains("FREETEXTTABLE(", StringComparison.Ordinal) && sql.Last.Contains("VECTOR_DISTANCE(", StringComparison.Ordinal)
    && sql.Last.Contains("FULL JOIN", StringComparison.Ordinal), "hybrid SQL uses FREETEXTTABLE, VECTOR_DISTANCE and FULL JOIN");
CheckHybridResults(hybrid, "hybrid");

Check.Summary();

Console.ReadLine();

static async Task<bool> WaitForPopulation(ApplicationDbContext db, int expectedRows)
{
    // PopulateStatus: 0 = idle, 1 = full population in progress, 9 = processing change tracking.
    // Typically a few seconds, longer under Rosetta emulation on Apple Silicon.
    for (var attempt = 0; attempt < 120; attempt++)
    {
        var status = await db.Database
            .SqlQueryRaw<int>("SELECT CAST(FULLTEXTCATALOGPROPERTY('ftCatalog', 'PopulateStatus') AS int) AS [Value]")
            .SingleAsync();
        var indexedRows = await db.Database
            .SqlQueryRaw<int>("SELECT CAST(OBJECTPROPERTYEX(OBJECT_ID('Articles'), 'TableFulltextItemCount') AS int) AS [Value]")
            .SingleAsync();
        Console.WriteLine($"  PopulateStatus {status}, indexed rows {indexedRows}/{expectedRows}");
        if (status == 0 && indexedRows == expectedRows)
        {
            return true;
        }

        await Task.Delay(500);
    }

    return false;
}

static void CheckHybridResults(List<HybridHit> results, string label)
{
    Check.That(results.Any(h => h is { FullTextPosition: not null, VectorPosition: null }), $"{label} results include an article only full-text search finds");
    Check.That(results.Any(h => h is { FullTextPosition: null, VectorPosition: not null }), $"{label} results include an article only vector search finds");
    var top = results.FirstOrDefault();
    Check.That(top is { FullTextPosition: not null, VectorPosition: not null }, $"the top {label} result is found by both searches");
    Check.Equal("Growing vegetables at home", top?.Title, $"top {label} result");
    Check.Descending(results.Select(h => h.Score), $"{label} results are in descending RRF score order");
}
