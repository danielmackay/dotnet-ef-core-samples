using Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EnhancedJsonColumns;

public class ApplicationDbContext : DbContext
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseSqlServer(DbConnectionFactory.Create("EnhancedJsonColumns"))
            // EF Core 11 marks owned JSON collections without a key as obsolete and logs
            // OwnedEntityMappedToJsonCollectionWarning. This sample keeps its owned-type demo, so the warning
            // is ignored. The documented key option (HasKey on the owned type) throws "Explicitly configured
            // primary keys are not supported on entities stored in JSON collections" in EF Core 11.0.0, and the
            // preferred EF 11 approach, ComplexCollection(...).ToJson(), is shown in the ComplexTypes11 sample.
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.OwnedEntityMappedToJsonCollectionWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().OwnsMany(e => e.Colors, builder => builder.ToJson());
    }
}