using Common;
using Microsoft.EntityFrameworkCore;

namespace Migrations11.Audit;

public class AuditDbContext : DbContext
{
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.UseSqlServer(DbConnectionFactory.Create("Migrations11Audit"));
}
