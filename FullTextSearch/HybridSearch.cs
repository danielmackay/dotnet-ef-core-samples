using Microsoft.Data.SqlTypes;
using Microsoft.EntityFrameworkCore;

namespace FullTextSearch;

/// <summary>
/// Hybrid search: a full-text search and a vector search for the same thing, merged with
/// FullJoin and scored with reciprocal rank fusion (RRF). Every method returns an IQueryable,
/// so the whole search runs on the server as one SQL statement.
/// </summary>
public static class HybridSearch
{
    /// <summary>The search text for the full-text side.</summary>
    public const string Text = "vegetables";

    /// <summary>How many results each search contributes before they're merged.</summary>
    public const int PerList = 3;

    /// <summary>The RRF smoothing constant. 60 is the value used in the original RRF paper.</summary>
    public const int RrfK = 60;

    /// <summary>The embedding of the same search for the vector side ("fresh produce from my garden").</summary>
    public static readonly SqlVector<float> Vector = SampleArticles.GardenProduce;

    // RRF scores each article by its *position* in each ranked list (1st, 2nd, 3rd...), not by the
    // raw full-text rank or vector distance, which are on different scales. EF can't translate
    // ROW_NUMBER(), so each position is 1 + the number of rows that rank better, counted by a
    // correlated subquery.

    /// <summary>The top full-text matches (FREETEXTTABLE), with their positions.</summary>
    public static IQueryable<RankedHit> FullTextHits(ApplicationDbContext db) =>
        db.Articles.Join(
            db.Articles.FreeTextTable<Article, int>(Text, topN: PerList),
            a => a.Id,
            f => f.Key,
            (a, f) => new RankedHit
            {
                Id = a.Id,
                Title = a.Title,
                Position = 1 + db.Articles.FreeTextTable<Article, int>(Text).Count(other => other.Rank > f.Rank),
            });

    /// <summary>
    /// The nearest articles with VectorSearch(...).WithApproximate() (VECTOR_SEARCH), as in the EF Core docs.
    /// EF Core 11 generates SQL that boxed SQL Server 2025 can't run yet (see readme).
    /// </summary>
    public static IQueryable<RankedHit> VectorSearchHits(ApplicationDbContext db) =>
        db.Articles
            .VectorSearch(a => a.Embedding, Vector, "cosine")
            .OrderBy(v => v.Distance)
            .Take(PerList)
            .WithApproximate()
            .Select(v => new RankedHit
            {
                Id = v.Value.Id,
                Title = v.Value.Title,
                // VectorSearch() can't be nested in a subquery, so count closer rows with VECTOR_DISTANCE.
                Position = 1 + db.Articles.Count(other => EF.Functions.VectorDistance("cosine", other.Embedding, Vector) < v.Distance),
            });

    /// <summary>The nearest articles with an exact VECTOR_DISTANCE search, which runs on every SQL Server 2025 build.</summary>
    public static IQueryable<RankedHit> VectorDistanceHits(ApplicationDbContext db) =>
        db.Articles
            .OrderBy(a => EF.Functions.VectorDistance("cosine", a.Embedding, Vector))
            .Take(PerList)
            .Select(a => new RankedHit
            {
                Id = a.Id,
                Title = a.Title,
                Position = 1 + db.Articles.Count(other =>
                    EF.Functions.VectorDistance("cosine", other.Embedding, Vector)
                    < EF.Functions.VectorDistance("cosine", a.Embedding, Vector)),
            });

    /// <summary>
    /// Merges the two ranked lists with FullJoin, so an article found by only one search still counts,
    /// and orders them by RRF score: the sum over the lists of 1 / (RrfK + position).
    /// </summary>
    // FullJoin passes the missing side as null. RankedHit is a struct, so that side arrives as a
    // RankedHit whose columns are all NULL, and the selector reads them directly, with no null
    // checks or `!`. (With a class, `t != null` can't be translated, and projecting entities with
    // ternaries, as the docs do, fails to materialize in EF Core 11.0.0.)
    public static IQueryable<HybridHit> Fuse(IQueryable<RankedHit> fullText, IQueryable<RankedHit> vector) =>
        fullText
            .FullJoin(
                vector,
                t => t.Id,
                v => v.Id,
                (t, v) => new
                {
                    // One of the two sides always exists, so the empty string is never used.
                    Title = t.Title ?? v.Title ?? string.Empty,
                    // A correlated COUNT(*) on the missing side gives 0, not NULL, so use the side's
                    // key to tell whether that search found the article.
                    FullTextPosition = t.Id == null ? null : t.Position,
                    VectorPosition = v.Id == null ? null : v.Position,
                })
            .Select(x => new
            {
                x.Title,
                x.FullTextPosition,
                x.VectorPosition,
                Score = (x.FullTextPosition == null ? 0.0 : 1.0 / (RrfK + x.FullTextPosition.Value))
                    + (x.VectorPosition == null ? 0.0 : 1.0 / (RrfK + x.VectorPosition.Value)),
            })
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Title)
            .Select(x => new HybridHit(x.Title, x.FullTextPosition, x.VectorPosition, x.Score));
}

/// <summary>
/// One search's result: the article and its position in that search's ranking (1 = best).
/// A struct, so FullJoin hands over the missing side as a RankedHit with NULL columns rather than null.
/// </summary>
public readonly struct RankedHit
{
    // Nullable so the FullJoin can tell a missing side apart (its Id is NULL).
    public int? Id { get; init; }

    public string? Title { get; init; }

    public int? Position { get; init; }
}

/// <summary>A hybrid search result: its position in each search (null = not found by it) and its RRF score.</summary>
public sealed record HybridHit(string Title, int? FullTextPosition, int? VectorPosition, double Score)
{
    public override string ToString() =>
        $"  {Score:F4}  {Title}  (full-text #{FullTextPosition?.ToString() ?? "-"}, vector #{VectorPosition?.ToString() ?? "-"})";
}
