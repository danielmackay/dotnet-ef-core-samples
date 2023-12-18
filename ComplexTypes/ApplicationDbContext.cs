using Common;
using Microsoft.EntityFrameworkCore;

namespace ComplexTypes;

public class ApplicationDbContext : DbContext
{
    public DbSet<Contact> Contacts { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseSqlServer(DbConnectionFactory.Create("ComplexTypes"));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Contact>()
            .ComplexProperty(c => c.Address);
    }
}