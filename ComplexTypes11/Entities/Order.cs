namespace ComplexTypes11.Entities;

public class Order
{
    public int Id { get; set; }

    // A complex collection stored as a json array column.
    public List<OrderLine> Lines { get; set; } = [];
}

public class OrderLine
{
    public required string Sku { get; set; }
    public int Quantity { get; set; }
}
