using Common;
using Microsoft.EntityFrameworkCore;

namespace JsonColumns;

public class ApplicationDbContext : DbContext
{
    public DbSet<Contact> Contacts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseSqlServer(DbConnectionFactory.Create("JsonColumns"));

        //optionsBuilder.LogTo(Console.WriteLine, new[] { RelationalEventId.CommandExecuted });
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Contact>()
            .OwnsOne(contact => contact.Address, builder => builder.ToJson())
            .OwnsMany(contact => contact.Notes, builder => builder.ToJson());
    }
}