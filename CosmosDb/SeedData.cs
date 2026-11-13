namespace CosmosDb;

public static class SeedData
{
    public static IEnumerable<Customer> Customers() =>
    [
        new()
        {
            Id = "alice",
            Region = "AU",
            Name = "Alice",
            Notes = "Prefers morning deliveries",
            ShippingAddress = new Address { Street = "1 George St", City = "Sydney", Postcode = "2000", Location = new Geo(-33.86, 151.21) },
            Orders = [new Order("A-1", 120.50m, "Shipped"), new Order("A-2", 35m, "Pending")],
        },
        new()
        {
            Id = "bob",
            Region = "AU",
            Name = "Bob",
            ShippingAddress = new Address { Street = "5 Collins St", City = "Melbourne", Postcode = "3000", Location = new Geo(-37.81, 144.96) },
            Orders = [new Order("B-1", 15m, "Pending")],
        },
        new()
        {
            Id = "carol",
            Region = "AU",
            Name = "Carol",
            ShippingAddress = new Address { Street = "9 Pitt St", City = "Sydney", Postcode = "2000", Location = new Geo(-33.87, 151.21) },
            Orders = [new Order("C-1", 410m, "Shipped")],
        },
        new()
        {
            Id = "dave",
            Region = "NZ",
            Name = "Dave",
            ShippingAddress = new Address { Street = "3 Queen St", City = "Auckland", Postcode = "1010", Location = new Geo(-36.85, 174.76) },
        },
    ];
}
