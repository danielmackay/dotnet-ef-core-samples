public class Product
{
    public required ProductId Id { get; set; }
    public required string Name { get; set; }
    public decimal Price { get; set; }
}

public record ProductId(Guid Value);
