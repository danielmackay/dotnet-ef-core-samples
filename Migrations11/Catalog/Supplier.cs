namespace Migrations11.Catalog;

public class Supplier
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public List<Product> Products { get; set; } = [];
}
