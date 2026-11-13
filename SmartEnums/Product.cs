using Ardalis.SmartEnum;
using System.Text.Json.Serialization;

namespace SmartEnums;

public class Product
{
    public int Id { get; set; }
    public required string Name { get; set; }

    public ProductType Type { get; set; }

    public List<ProductCategory> Categories { get; set; } = [];

    public override string ToString()
    {
        return $"{Id}: {Name} - {Type.Name} - {string.Join(", ", Categories.Select(c => c.Name))}";
    }
}

public class ProductType : SmartEnum<ProductType, Guid>
{
    public static readonly ProductType Physical = new("Physical", Guid.NewGuid());
    public static readonly ProductType Digital = new("Digital", Guid.NewGuid());
    public static readonly ProductType Service = new("Service", Guid.NewGuid());

    private ProductType(string name, Guid value) : base(name, value)
    {
    }
}

public class ProductCategory : SmartEnum<ProductCategory, int>
{
    public static readonly ProductCategory Electronics = new("Electronics", 1);
    public static readonly ProductCategory Clothing = new("Clothing", 2);
    public static readonly ProductCategory Food = new("Food", 3);
    public static readonly ProductCategory Books = new("Books", 4);
    public static readonly ProductCategory HomeGoods = new("Home Goods", 5);

    private ProductCategory(string name, int value) : base(name, value)
    {
    }

    // [JsonConstructor]
    // public static ProductCategory Create(int value) => ProductCategory.FromValue(value);
}