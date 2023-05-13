using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseSqlServer(
                @"Server=(localdb)\mssqllocaldb;Database=Sequences;Trusted_Connection=True");

        //optionsBuilder.LogTo(Console.WriteLine, new[] { RelationalEventId.CommandExecuted });
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasSequence<int>("OrderNumbers")
            .StartsAt(100)
            .IncrementsBy(10);

        modelBuilder
            .Entity<Order>()
            .Property(o => o.OrderNumber)
            .HasDefaultValueSql("NEXT VALUE FOR OrderNumbers");
    }
}
