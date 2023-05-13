public class Product
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public decimal Price { get; set; }

    // As there is no FK, this will create a shadow property for the relationship
    public Customer Customer { get; set; }
}

public class Customer
{
    public int Id { get; set; }
    public required string Name { get; set; }
}
