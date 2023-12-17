using Common;
using CompiledModels.CompiledModels;
using Microsoft.EntityFrameworkCore;

namespace CompiledModels;

public class ApplicationDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseSqlServer(DbConnectionFactory.Create("CompiledModels"))
            .UseModel(ApplicationDbContextModel.Instance);

        //optionsBuilder.LogTo(Console.WriteLine, new[] { RelationalEventId.CommandExecuted });
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Product>()
            .Property(p => p.Price)
            .HasPrecision(18, 2);
    }
}