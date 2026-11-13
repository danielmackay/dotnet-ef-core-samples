using Common;
using Microsoft.EntityFrameworkCore;

namespace FullTextSearch;

public class ApplicationDbContext(SqlCapture sqlCapture) : DbContext
{
    public const string CatalogName = "ftCatalog";

    // Used by the dotnet-ef CLI at design time.
    public ApplicationDbContext() : this(new SqlCapture())
    {
    }

    public DbSet<Article> Articles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseSqlServer(DbConnectionFactory.Create("FullTextSearch"))
            .AddInterceptors(sqlCapture);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // The catalog would normally be declared in the model too:
        //
        //     modelBuilder.HasFullTextCatalog(CatalogName);
        //
        // In EF Core 11.0.0 that breaks migrations (dotnet/efcore#39599, see readme), so the
        // InitialCreate migration creates the catalog with a line of SQL instead.

        var article = modelBuilder.Entity<Article>();

        // Columns in a full-text index are sized like index key columns (nvarchar(450)) unless told
        // otherwise. Full-text search handles max types, so keep article bodies unbounded.
        article.Property(a => a.Contents).HasColumnType("nvarchar(max)");
        article.Property(a => a.Title).HasMaxLength(200);

        // Hybrid search compares these embeddings with VECTOR_DISTANCE / VECTOR_SEARCH.
        article.Property(a => a.Embedding).HasColumnType($"vector({SampleArticles.Dimensions})");

        article
            .HasFullTextIndex(a => new { a.Title, a.Contents })
            .UseKeyIndex("PK_Articles")
            .UseCatalog(CatalogName);
    }
}
