namespace CosmosDb;

public class Customer
{
    public required string Id { get; set; }

    /// <summary>The partition key. Documents for one region share a logical partition.</summary>
    public required string Region { get; set; }

    public required string Name { get; set; }

    /// <summary>Free text that's never filtered on, so it's excluded from the index.</summary>
    public string? Notes { get; set; }

    /// <summary>A complex type: stored as a nested JSON object inside the customer document.</summary>
    public required Address ShippingAddress { get; set; }

    /// <summary>A complex collection: stored as a JSON array of objects inside the customer document.</summary>
    public List<Order> Orders { get; set; } = [];

    public override string ToString() =>
        $"{Name} ({Region}) - {ShippingAddress}, {Orders.Count} order(s): {string.Join(", ", Orders)}";
}

/// <summary>
/// Complex types have value semantics and no key. Records fit well: EF compares them by value, and
/// changing a nested value means assigning a new instance (<c>address with { City = ... }</c>).
/// </summary>
/// <remarks>
/// Init-only properties rather than a positional record: EF can't bind a nested complex type (Location)
/// to a constructor parameter.
/// </remarks>
public record Address
{
    public required string Street { get; init; }
    public required string City { get; init; }
    public required string Postcode { get; init; }
    public required Geo Location { get; init; }

    public override string ToString() => $"{Street}, {City} {Postcode} [{Location}]";
}

/// <summary>A complex type nested inside another complex type.</summary>
public record Geo(double Latitude, double Longitude)
{
    public override string ToString() => $"{Latitude}, {Longitude}";
}

public record Order(string Number, decimal Total, string Status)
{
    public override string ToString() => $"{Number} {Total:C} {Status}";
}
