using Common;
using Microsoft.EntityFrameworkCore;

namespace Migrations11.Catalog;

public class CatalogDbContext : DbContext
{
    private readonly SqlCapture? _sql;

    // The dotnet ef tools create the context through this constructor.
    public CatalogDbContext()
    {
    }

    public CatalogDbContext(SqlCapture sql)
    {
        _sql = sql;
    }

    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(DbConnectionFactory.Create("Migrations11"));

        if (_sql is not null)
            optionsBuilder.AddInterceptors(_sql);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(product =>
        {
            product.Property(p => p.Price).HasPrecision(18, 2);

            // Suppliers are synced in by another system, so a product can arrive before its supplier.
            // EF still treats this as a relationship (navigations, Include, the index on SupplierId),
            // but migrations don't create the FK constraint that would reject those rows.
            product.HasOne(p => p.Supplier)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.SupplierId)
                .ExcludeForeignKeyFromMigrations();
        });
    }
}
