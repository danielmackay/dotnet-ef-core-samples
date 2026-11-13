using Common;
using Microsoft.EntityFrameworkCore;

namespace TemporalTables;

public class ApplicationDbContext(SqlCapture sqlCapture) : DbContext
{
    public DbSet<Product> Products { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseSqlServer(DbConnectionFactory.Create("TemporalTables"))
            .AddInterceptors(sqlCapture);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Product>()
            .ToTable("Products", b => b.IsTemporal(t =>
            {
                // EF Core 11: lambda overloads map the period columns to CLR properties.
                // Before EF 11 they could only be shadow properties.
                t.HasPeriodStart(p => p.PeriodStart);
                t.HasPeriodEnd(p => p.PeriodEnd);
            }));
    }
}
