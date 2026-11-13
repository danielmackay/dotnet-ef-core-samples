using Common;
using ComplexTypes11.Entities;
using Microsoft.EntityFrameworkCore;

namespace ComplexTypes11;

public class ApplicationDbContext(SqlCapture sqlCapture) : DbContext
{
    public DbSet<Animal> Animals => Set<Animal>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();

    // Parameterless constructor for `dotnet ef migrations add`.
    public ApplicationDbContext() : this(new SqlCapture())
    {
    }

    /// <summary>The database both of the sample's contexts use.</summary>
    public const string DatabaseName = "ComplexTypes11";

    /// <summary>Compatibility 170 is SQL Server 2025: ToJson() then maps to the native json type by default.</summary>
    public const int CompatibilityLevel = 170;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseSqlServer(DbConnectionFactory.Create(DatabaseName), o => o.UseCompatibilityLevel(CompatibilityLevel))
            .AddInterceptors(sqlCapture);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Animal>().UseTptMappingStrategy().ToTable("Animals");
        modelBuilder.Entity<Dog>().ToTable("Dogs");
        modelBuilder.Entity<Cat>().ToTable("Cats");
        modelBuilder.Entity<Animal>().ComplexProperty(a => a.Details);
        modelBuilder.Entity<Dog>().ComplexProperty(d => d.Extras, b => b.ToJson());

        modelBuilder.Entity<Vehicle>().UseTpcMappingStrategy();
        modelBuilder.Entity<Car>().ToTable("Cars");
        modelBuilder.Entity<Truck>().ToTable("Trucks");
        modelBuilder.Entity<Vehicle>().ComplexProperty(v => v.Spec);
        modelBuilder.Entity<Car>().ComplexProperty(c => c.Features, b => b.ToJson());

        // EF 11: reach into the complex type straight from Property(...). Before, this needed
        // ComplexProperty(v => v.Spec, b => b.Property(s => s.Make).HasMaxLength(50)).
        modelBuilder.Entity<Vehicle>().Property(v => v.Spec.Make).HasMaxLength(50);

        modelBuilder.Entity<Customer>().ComplexProperty(c => c.Number);
        modelBuilder.Entity<Customer>().ComplexProperty(c => c.Address);
        modelBuilder.Entity<Customer>().ComplexProperty(c => c.Contact, b => b.ToJson().HasColumnType("json"));
        modelBuilder.Entity<Customer>().HasKey(c => c.Number.Value);

        // Indexes on complex-type properties, by lambda and by name (the dotted path through the complex type).
        modelBuilder.Entity<Customer>().HasIndex(c => c.Address.PostalCode);
        modelBuilder.Entity<Customer>().HasIndex("Address.Street");

        // A path inside a json column becomes a SQL Server 2025 JSON index (CREATE JSON INDEX ... FOR ('$.Address.City')).
        // SQL Server allows one JSON index per json column, so a second HasIndex into Contact would fail.
        modelBuilder.Entity<Customer>().HasIndex("Contact.Address.City");

        modelBuilder.Entity<Order>().ComplexCollection(o => o.Lines, b => b.ToJson().HasColumnType("json"));
    }
}
