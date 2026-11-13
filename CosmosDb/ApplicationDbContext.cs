using Common;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace CosmosDb;

/// <param name="capture">Records the Cosmos SQL and item operations this context runs.</param>
/// <param name="configureCosmos">Per-demo Cosmos options, e.g. bulk execution or session-token mode.</param>
public class ApplicationDbContext(
    CosmosCapture capture,
    Action<CosmosDbContextOptionsBuilder>? configureCosmos = null) : DbContext
{
    public const string DatabaseName = "CosmosDb";
    public const string CustomersContainer = "Customers";

    /// <summary>
    /// Whether the model includes the composite index over the Orders[] array. Azure Cosmos DB supports
    /// array paths in composite indexes, but the Linux emulator rejects the whole container. Program sets
    /// this from <see cref="EmulatorCapabilities"/> before the first context builds the (cached) model.
    /// </summary>
    public static bool IncludeArrayCompositeIndex { get; set; } = true;

    public DbSet<Customer> Customers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseCosmos(DbConnectionFactory.CreateCosmos(), DatabaseName, cosmos =>
        {
            // The emulator only exposes its gateway endpoint, so don't try to discover other regions or use
            // direct (TCP) mode.
            cosmos.ConnectionMode(ConnectionMode.Gateway);
            cosmos.LimitToEndpoint();
            configureCosmos?.Invoke(cosmos);
        });
        capture.Attach(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(customer =>
        {
            customer.ToContainer(CustomersContainer);
            customer.HasPartitionKey(c => c.Region);

            customer.ComplexProperty(c => c.ShippingAddress);
            customer.ComplexCollection(c => c.Orders);

            // Cosmos indexes every path by default. Keep that, but stop paying RU for indexing a property
            // that's never filtered on. Paths are Cosmos JSON paths, so they use the JSON property names.
            customer.HasAutomaticIndexing().Except("/Notes/?");

            // A multi-property HasIndex becomes a Cosmos composite index, needed to ORDER BY several
            // properties. EF 11 allows index properties inside complex types...
            customer.HasIndex(c => new { c.ShippingAddress.City, c.Name });

            // ...and inside complex collections, named with "[]". EF maps this to the /Orders/[]/Total path,
            // which helps queries like "orders over $100 in a region".
            if (IncludeArrayCompositeIndex)
            {
                customer.HasIndex("Region", "Orders[].Total").IsDescending(false, true);
            }
        });
    }
}
