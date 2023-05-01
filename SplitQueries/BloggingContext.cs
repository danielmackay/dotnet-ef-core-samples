using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace SplitQueries;

public class BloggingContext : DbContext
{
    private readonly bool _useSplitQueries;

    public DbSet<Blog> Blogs { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Tag> Tags { get; set; }

    public BloggingContext(bool useSplitQueries)
    {
        _useSplitQueries = useSplitQueries;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var splitQueryBehavior = _useSplitQueries ? QuerySplittingBehavior.SplitQuery : QuerySplittingBehavior.SingleQuery;

        optionsBuilder
            .UseSqlServer(
                @"Server=(localdb)\mssqllocaldb;Database=SplitQueries;Trusted_Connection=True",
                options => options.UseQuerySplittingBehavior(splitQueryBehavior));

        optionsBuilder.LogTo(Console.WriteLine, new[] { RelationalEventId.CommandExecuted });
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Post>()
            .HasOne(p => p.Blog)
            .WithMany(p => p.Posts)
            .HasForeignKey(p => p.BlogId);

        base.OnModelCreating(modelBuilder);
    }
}