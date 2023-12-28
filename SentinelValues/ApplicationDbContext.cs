using Common;
using Microsoft.EntityFrameworkCore;

namespace EnhancedJsonColumns;

public class ApplicationDbContext : DbContext
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseSqlServer(DbConnectionFactory.Create("EnhancedJsonColumns"));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().OwnsMany(e => e.Colors, builder => builder.ToJson());
    }
}