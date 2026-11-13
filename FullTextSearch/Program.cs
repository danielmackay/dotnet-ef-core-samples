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
    fullTextInstalled = (int?)await command.ExecuteScalarAsync();
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
Check.That(script.Contains("CREATE FULLTEXT CATALOG [ftCatalog]", StringComparison.Ordinal), "migration creates the ftCatalog full-text catalog");
Check.That(script.Contains("CREATE FULLTEXT INDEX ON [Articles]([Title], [Contents]) KEY INDEX [PK_Articles] ON [ftCatalog]", StringComparison.Ordinal),
    "migration creates the full-text index on Title and Contents, keyed by PK_Articles, in ftCatalog");

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
db.Articles.AddRange(
    new Article { Title = "Growing vegetables at home", Contents = "A beginner's guide to growing your own vegetables. Vegetables need sun, water and good soil, so plant them in spring." },
    new Article { Title = "Fruit salad", Contents = "Apples, pears and berries make a fresh fruit salad for summer." },
    new Article { Title = "Car engine maintenance", Contents = "Change the oil and check the engine belts every year." },
    new Article { Title = "Winter soup", Contents = "A hearty soup of carrots, leeks and other winter vegetables." },
    new Article { Title = "Healthy eating", Contents = "Eat fruit every day and add greens to every meal." });
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
Check.That(IsDescending(freeText.Select(x => x.Rank)), "FREETEXTTABLE results are in descending rank order");

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
Check.That(IsDescending(contains.Select(x => x.Rank)), "CONTAINSTABLE results are in descending rank order");

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

static bool IsDescending(IEnumerable<int> ranks) =>
    ranks.Zip(ranks.Skip(1)).All(pair => pair.First >= pair.Second);
