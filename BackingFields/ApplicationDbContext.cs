using Common;
using Microsoft.EntityFrameworkCore;

namespace BackingFields;

public class ApplicationDbContext : DbContext
{
    public DbSet<Product> Products { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseSqlServer(DbConnectionFactory.Create("BackingFields"));

        //optionsBuilder.LogTo(Console.WriteLine, new[] { RelationalEventId.CommandExecuted });
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Product>()
            .Property(p => p.VoucherCode)
            .HasField("_voucherCode");

        modelBuilder
            .Entity<Product>()
            .Property("_createdTs")
            .HasColumnName("CreatedTs");
    }
}