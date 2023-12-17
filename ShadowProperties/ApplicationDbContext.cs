using Common;
using Microsoft.EntityFrameworkCore;

namespace ShadowProperties;

public class ApplicationDbContext : DbContext
{
    public DbSet<Product> Products { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseSqlServer(DbConnectionFactory.Create("ShadowProperties"));

        //optionsBuilder.LogTo(Console.WriteLine, new[] { RelationalEventId.CommandExecuted });
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Shadow property
        modelBuilder
            .Entity<Product>()
            .Property<DateTime>("CreatedTs")
            .HasDefaultValue(DateTime.UtcNow);
    }
}