using Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SmartEnum.EFCore;

namespace SmartEnums;

public class ApplicationDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseSqlServer(DbConnectionFactory.Create("SmartEnums"));

        //optionsBuilder.LogTo(Console.WriteLine, new[] { RelationalEventId.CommandExecuted });
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Mapping the list as a primitive collection (rather than converting the whole list) stores it
        // as a JSON array, so EF can translate queries such as Categories.Contains(...) to OPENJSON.
        // EF Core 11's Property() no longer configures primitive collections, so PrimitiveCollection() is required.
        modelBuilder.Entity<Product>()
            .PrimitiveCollection(p => p.Categories)
            .ElementType(e => e.HasConversion(
                new ValueConverter<ProductCategory, int>(c => c.Value, v => ProductCategory.FromValue(v))));
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.ConfigureSmartEnum();
    }
}