using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace TablePerType.Complex;

public class ComplexDbContext : DbContext
{
    public DbSet<FarmAnimal> FarmAnimals { get; set; }

    public DbSet<Cat> Cats { get; set; }

    public DbSet<Dog> Dogs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseSqlServer(
                @"Server=(localdb)\mssqllocaldb;Database=ComplexTpt;Trusted_Connection=True");

        optionsBuilder.LogTo(Console.WriteLine, new[] { RelationalEventId.CommandExecuted });
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Animal>().UseTptMappingStrategy();
        modelBuilder.Entity<Pet>().UseTptMappingStrategy();
    }
}