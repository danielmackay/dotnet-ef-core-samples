using Common;
using Microsoft.EntityFrameworkCore;

namespace VectorSearch;

public class ApplicationDbContext(SqlCapture? sqlCapture = null) : DbContext
{
    public DbSet<Article> Articles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(DbConnectionFactory.Create("VectorSearch"));

        if (sqlCapture is not null)
            optionsBuilder.AddInterceptors(sqlCapture);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Article>()
            .Property(a => a.Embedding)
            .HasColumnType($"vector({SampleArticles.Dimensions})");

        // The second argument of HasVectorIndex is the index name, not the metric: the metric
        // always goes through HasMetric. The metric must match the one VectorSearch() asks for.
        modelBuilder.Entity<Article>()
            .HasVectorIndex(a => a.Embedding)
            .HasMetric("cosine");
    }
}
