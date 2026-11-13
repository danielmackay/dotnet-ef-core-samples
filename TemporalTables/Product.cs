namespace TemporalTables;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }

    // EF Core 11: the temporal period columns can be mapped to real CLR properties,
    // so they can be read and queried without EF.Property<DateTime>(p, "PeriodStart").
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }

    public override string ToString()
    {
        return $"Id: {Id}, Name: {Name}, Price: {Price}, Period: {PeriodStart:HH:mm:ss.fff} - {PeriodEnd:yyyy-MM-dd HH:mm:ss}";
    }
}
