namespace Migrations11.Catalog;

public class Product
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public decimal Price { get; set; }

    // Added by the AddProductRating migration, the second catalog migration (see the readme).
    public int? Rating { get; set; }

    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
}
