using Common;
using Microsoft.EntityFrameworkCore;

namespace EnhancedBulkUpdateAndDelete;

public class ApplicationDbContext : DbContext
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseSqlServer(DbConnectionFactory.Create("EnhancedBulkUpdateAndDelete"));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().OwnsOne(p => p.Color);
    }
}