using Common;
using Microsoft.EntityFrameworkCore;

namespace JsonFunctions;

public class ApplicationDbContext(SqlCapture sqlCapture, int compatibilityLevel = 170) : DbContext
{
    public DbSet<Product> Products { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseSqlServer(
                DbConnectionFactory.Create("JsonFunctions"),
                o => o.UseCompatibilityLevel(compatibilityLevel))
            .AddInterceptors(sqlCapture);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // JSON_CONTAINS only accepts the native json type: on nvarchar(max) SQL Server rejects the call.
        // Plain string properties stay nvarchar(max) unless told otherwise, even at compatibility 170.
        modelBuilder.Entity<Product>()
            .Property(p => p.JsonData)
            .HasColumnType("json");
    }
}
