using Common;
using ComplexTypes11.Entities;
using Microsoft.EntityFrameworkCore;

namespace ComplexTypes11;

/// <summary>
/// Shows the index EF 11 generates for a <c>[]</c> path into a JSON complex collection. It's only
/// scripted, never applied: SQL Server 2025 rejects the resulting <c>$[*]</c> JSON index path
/// (Msg 13683), although Cosmos DB accepts <c>[]</c> paths in its indexes.
/// </summary>
public class CollectionIndexContext : DbContext
{
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(
            DbConnectionFactory.Create(ApplicationDbContext.DatabaseName),
            o => o.UseCompatibilityLevel(ApplicationDbContext.CompatibilityLevel));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>().ComplexCollection(o => o.Lines, b => b.ToJson().HasColumnType("json"));

        // [] means "every element of the collection".
        modelBuilder.Entity<Order>().HasIndex("Lines[].Sku");
    }
}
