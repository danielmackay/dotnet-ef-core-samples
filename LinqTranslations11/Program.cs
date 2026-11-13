using Common;
using LinqTranslations11;
using Microsoft.EntityFrameworkCore;

Console.WriteLine("LINQ Translations in EF Core 11 Sample");

var sql = new SqlCapture();
await using var db = new ApplicationDbContext(sql);
await db.Database.EnsureDeletedAsync();
await db.Database.EnsureCreatedAsync();
await Seed(db);

await FullJoin();
await MaxByMinBy();
await GroupByImprovements();
await ToOneJoins();
await NoOpCasts();
Check.Summary();

Console.ReadLine();

async Task FullJoin()
{
    Header("1. FullJoin -> FULL JOIN");
    sql.Clear();
    var rows = await db.Customers
        .FullJoin(
            db.Orders,
            c => c.Id,
            o => o.CustomerId,
            (c, o) => new { Customer = c != null ? c.Name : null, OrderId = o != null ? (int?)o.Id : null })
        .ToListAsync();
    rows.ForEach(r => Console.WriteLine($"Customer: {r.Customer ?? "(none)"}, Order: {r.OrderId?.ToString() ?? "(none)"}"));
    Console.WriteLine(sql.Last);

    Check.Equal(4, rows.Count, "one row per matched order, plus the customer with no orders and the orphan order");
    Check.That(rows.Any(r => r.Customer == "Carol" && r.OrderId == null), "customer with no orders is kept");
    Check.That(rows.Any(r => r.Customer == null && r.OrderId != null), "order with no customer is kept");
    Check.That(sql.Last.Contains("FULL JOIN", StringComparison.Ordinal), "SQL uses FULL JOIN");
}

async Task MaxByMinBy()
{
    Header("2. MaxByAsync / MinByAsync -> TOP(1) ... ORDER BY");
    sql.Clear();
    var busiest = await db.Blogs.MaxByAsync(b => b.Posts.Count());
    Console.WriteLine($"Blog with the most posts: {busiest?.Name}");
    Console.WriteLine(sql.Last);
    Check.Equal("EF Core Blog", busiest?.Name, "MaxByAsync returns the blog with the most posts");
    Check.Equal(1, sql.Commands.Count, "MaxByAsync runs a single query");
    Check.That(sql.Last.Contains("TOP(1)", StringComparison.Ordinal), "MaxByAsync SQL uses TOP(1)");
    Check.That(sql.Last.Contains(") DESC", StringComparison.Ordinal), "MaxByAsync SQL orders by the key descending");

    sql.Clear();
    var quietest = await db.Blogs.MinByAsync(b => b.Posts.Count());
    Console.WriteLine($"Blog with the fewest posts: {quietest?.Name}");
    Console.WriteLine(sql.Last);
    Check.Equal("Empty Blog", quietest?.Name, "MinByAsync returns the blog with the fewest posts");
    Check.Equal(1, sql.Commands.Count, "MinByAsync runs a single query");
    Check.That(sql.Last.Contains("TOP(1)", StringComparison.Ordinal), "MinByAsync SQL uses TOP(1)");
    Check.That(sql.Last.Contains("ORDER BY", StringComparison.Ordinal) && !sql.Last.Contains("DESC", StringComparison.Ordinal),
        "MinByAsync SQL orders by the key ascending");
}

async Task GroupByImprovements()
{
    Header("3a. GroupBy: navigation over the top row per group");
    sql.Clear();
    // Composing a navigation over the "top row per group" entity used to fail with
    // "Translation of member 'Blog' failed". EF 11 expands it into a regular join.
    var topBlogs = await db.Posts
        .GroupBy(p => p.Category)
        .Select(g => g.OrderByDescending(p => p.Likes).First())
        .Select(p => new { p.Category, TopBlog = p.Blog!.Name })
        .OrderBy(x => x.Category)
        .ToListAsync();
    topBlogs.ForEach(x => Console.WriteLine($"{x.Category}: most-liked post is on {x.TopBlog}"));
    Console.WriteLine(sql.Last);
    Check.Equal(2, topBlogs.Count, "one row per category");
    Check.Equal("EF Core Blog", topBlogs[0].TopBlog, "Features: most-liked post is on the EF Core Blog");
    Check.Equal("Aspire Blog", topBlogs[1].TopBlog, "Releases: most-liked post is on the Aspire Blog");
    Check.Equal(1, sql.Commands.Count, "runs as a single server-side query");
    Check.That(sql.Last.Contains("ROW_NUMBER() OVER(PARTITION BY", StringComparison.Ordinal), "SQL picks the top row per group with ROW_NUMBER()");
    Check.That(sql.Last.Contains("JOIN [Blogs]", StringComparison.Ordinal), "SQL joins Blogs to reach the top row's navigation");

    Header("3b. GroupBy: aggregate over a reference navigation");
    sql.Clear();
    var ratings = await db.Posts
        .GroupBy(p => p.Category)
        .Select(g => new { Category = g.Key, TotalBlogRating = g.Sum(p => p.Blog!.Rating) })
        .OrderBy(x => x.Category)
        .ToListAsync();
    ratings.ForEach(x => Console.WriteLine($"{x.Category}: total blog rating {x.TotalBlogRating}"));
    Console.WriteLine(sql.Last);
    Check.Equal(10, ratings[0].TotalBlogRating, "Features: 5 + 5");
    Check.Equal(8, ratings[1].TotalBlogRating, "Releases: 5 + 3");
    Check.That(sql.Last.Contains("SUM([b].[Rating])", StringComparison.Ordinal), "SQL sums the joined Blogs column directly");
    Check.That(sql.Last.Contains("JOIN [Blogs]", StringComparison.Ordinal) && sql.Last.Contains("GROUP BY", StringComparison.Ordinal),
        "SQL joins Blogs before grouping (no correlated subquery)");
}

async Task ToOneJoins()
{
    Header("4a. Split query: the reference navigation is no longer joined in the collection query");
    db.ChangeTracker.Clear();
    sql.Clear();
    var split = await db.Blogs
        .Include(b => b.Owner)
        .Include(b => b.Posts)
        .AsSplitQuery()
        .OrderBy(b => b.Id)
        .ToListAsync();
    split.ForEach(b => Console.WriteLine($"{b.Name} (owner {b.Owner?.Name}): {b.Posts.Count} posts"));
    sql.Commands.ToList().ForEach(c => Console.WriteLine(c + Environment.NewLine));
    Check.That(split.All(b => b.Owner is not null), "every blog has its owner loaded");
    Check.That(split.Select(b => b.Posts.Count).SequenceEqual([3, 1, 0]), "every blog has its posts loaded");
    Check.Equal(2, sql.Commands.Count, "split query runs one query for blogs + owner, one for posts");
    var collectionQuery = sql.Commands.FirstOrDefault(c => c.Contains("JOIN [Posts]", StringComparison.Ordinal)) ?? string.Empty;
    Check.That(collectionQuery.Length > 0, "found the posts collection query");
    Check.That(!collectionQuery.Contains("[People]", StringComparison.Ordinal), "posts query does not join People (the owner)");

    Header("4b. Single query: the reference key is gone from ORDER BY");
    db.ChangeTracker.Clear();
    sql.Clear();
    var single = await db.Blogs
        .Include(b => b.Owner)
        .Include(b => b.Posts)
        .ToListAsync();
    Console.WriteLine(sql.Last);
    Check.Equal(3, single.Count, "single query returns every blog");
    Check.Equal(4, single.Sum(b => b.Posts.Count), "single query loads every post");
    var ownerAlias = AliasOf(sql.Last, "People");
    var orderBy = sql.Last[(sql.Last.LastIndexOf("ORDER BY", StringComparison.Ordinal) + "ORDER BY".Length)..];
    Check.That(ownerAlias.Length > 0 && sql.Last.Contains("ORDER BY", StringComparison.Ordinal), "single query joins People and has an ORDER BY");
    Check.That(!orderBy.Contains($"{ownerAlias}.", StringComparison.Ordinal), $"ORDER BY does not include the owner key ({ownerAlias}.[Id])");
}

async Task NoOpCasts()
{
    Header("5. No-op CAST stripped from a value-converted column");
    sql.Clear();
    // The (string) cast uses Email's implicit conversion; EF used to emit
    // CAST([c].[Email] AS nvarchar(max)) even though the column already is nvarchar(max).
    var matches = await db.Customers
        .Where(c => ((string)c.Email).StartsWith("alice"))
        .ToListAsync();
    matches.ForEach(c => Console.WriteLine($"{c.Name} <{c.Email.Value}>"));
    Console.WriteLine(sql.Last);
    Check.That(matches.Select(c => c.Name).SequenceEqual(["Alice"]), "filter on the converted column finds Alice");
    Check.That(sql.Last.Contains("[c].[Email] LIKE", StringComparison.Ordinal), "SQL compares the column directly");
    Check.That(!sql.Last.Contains("CAST(", StringComparison.Ordinal), "SQL has no CAST( around the converted column");
}

// Finds the alias SQL Server gives a table, e.g. "[People] AS [p]" -> "[p]".
static string AliasOf(string sqlText, string table)
{
    var marker = $"[{table}] AS ";
    var start = sqlText.IndexOf(marker, StringComparison.Ordinal);
    if (start < 0) return string.Empty;
    start += marker.Length;
    var end = sqlText.IndexOf(']', start);
    return sqlText[start..(end + 1)];
}

static async Task Seed(ApplicationDbContext db)
{
    db.Customers.AddRange(
        new Customer { Id = 1, Name = "Alice", Email = new("alice@example.com") },
        new Customer { Id = 2, Name = "Bob", Email = new("bob@example.com") },
        new Customer { Id = 3, Name = "Carol", Email = new("carol@example.com") });
    db.Orders.AddRange(
        new Order { Id = 1, CustomerId = 1, Total = 10m },
        new Order { Id = 2, CustomerId = 2, Total = 20m },
        new Order { Id = 3, CustomerId = 99, Total = 30m });

    var dan = new Person { Name = "Dan" };
    var sam = new Person { Name = "Sam" };
    db.Blogs.AddRange(
        new Blog
        {
            Name = "EF Core Blog",
            Rating = 5,
            Owner = dan,
            Posts =
            [
                new Post { Title = "What's new in EF Core 11", Category = "Releases", Likes = 50 },
                new Post { Title = "Split queries", Category = "Features", Likes = 20 },
                new Post { Title = "FullJoin", Category = "Features", Likes = 30 },
            ]
        },
        new Blog
        {
            Name = "Aspire Blog",
            Rating = 3,
            Owner = sam,
            Posts = [new Post { Title = "Aspire 13", Category = "Releases", Likes = 60 }]
        },
        new Blog { Name = "Empty Blog", Rating = 1, Owner = sam });

    await db.SaveChangesAsync();
}

static void Header(string title)
{
    Console.WriteLine();
    Console.WriteLine($"=== {title} ===");
}
