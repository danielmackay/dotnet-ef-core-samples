using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace TablePerHierarchy.Simple;

public class SimpleDbContext : DbContext
{
    public DbSet<Animal> Animals { get; set; }

    public DbSet<Pet> Pets { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseSqlServer(
                @"Server=(localdb)\mssqllocaldb;Database=SimpleTph;Trusted_Connection=True");

        optionsBuilder.LogTo(Console.WriteLine, new[] { RelationalEventId.CommandExecuted });
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }
}