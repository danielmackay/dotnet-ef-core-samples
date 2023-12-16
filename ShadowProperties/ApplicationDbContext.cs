using Microsoft.EntityFrameworkCore;

namespace ShadowProperties;

public class ApplicationDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseSqlServer(
                @"Server=(localdb)\mssqllocaldb;Database=ShadowProperties;Trusted_Connection=True");

        //optionsBuilder.LogTo(Console.WriteLine, new[] { RelationalEventId.CommandExecuted });
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Shadow property
        modelBuilder
            .Entity<Product>()
            .Property<DateTime>("CreatedTs")
            .HasDefaultValue(DateTime.UtcNow);
    }
}