using Common;
using Microsoft.EntityFrameworkCore;
using SmartEnum.EFCore;
using System.Collections.Immutable;

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
        // modelBuilder
        //     .Entity<Product>()
        //     // .Property(p => p.Categories)
        //     // .Property(p => p.Name)
        //     // .PrimitiveCollection(c => c.Categories);
        //     .OwnsMany(c => c.Categories, builder => builder.ToJson());

        modelBuilder.Entity<Product>()
            .Property(p => p.Categories)
            .HasConversion(
                v => v.Select(c => c.Value),
                v => v.Select(ProductCategory.FromValue).ToList());
        // .OwnsOne(c => c.Address);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.ConfigureSmartEnum();
    }
}