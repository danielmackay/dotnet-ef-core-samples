using Common;
using Microsoft.EntityFrameworkCore;

namespace PrimitiveCollections;

public class ApplicationDbContext : DbContext
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseSqlServer(DbConnectionFactory.Create("PrimitiveCollections"));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }
}