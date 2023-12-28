using Common;
using Microsoft.EntityFrameworkCore;

namespace HierarchyIds;

public class ApplicationDbContext : DbContext
{
    public DbSet<Employee> Employees => Set<Employee>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseSqlServer(DbConnectionFactory.Create("HierarchyIds"), builder => builder.UseHierarchyId());
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }
}