using Common;
using Microsoft.EntityFrameworkCore;

namespace SentinelValues;

public class ApplicationDbContext : DbContext
{
    public DbSet<Account> Accounts => Set<Account>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseSqlServer(DbConnectionFactory.Create("SentinelValues"));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>().Property(a => a.Credits).HasDefaultValue(10).HasSentinel(-1);
    }
}