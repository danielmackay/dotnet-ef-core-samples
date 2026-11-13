using Common;
using Microsoft.EntityFrameworkCore;

namespace LinqTranslations11;

public class ApplicationDbContext(SqlCapture sqlCapture) : DbContext
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Blog> Blogs => Set<Blog>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Person> People => Set<Person>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseSqlServer(DbConnectionFactory.Create("LinqTranslations11"))
            .AddInterceptors(sqlCapture);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Fixed IDs keep the seed data (and so the checks) the same on every run.
        modelBuilder.Entity<Customer>().Property(c => c.Id).ValueGeneratedNever();
        modelBuilder.Entity<Customer>()
            .Property(c => c.Email)
            .HasConversion(e => e.Value, v => new Email(v));
        modelBuilder.Entity<Order>().Property(o => o.Id).ValueGeneratedNever();
        modelBuilder.Entity<Order>().Property(o => o.Total).HasPrecision(18, 2);
    }
}
