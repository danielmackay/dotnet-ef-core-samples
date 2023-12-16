namespace UnmappedTypes;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public bool IsDeleted { get; set; }

    public override string ToString()
    {
        return $"Id: {Id}, Name: {Name}, IsDeleted: {IsDeleted}";
    }
}

public class UnmappedProduct
{
    public string Name { get; set; } = null!;

    public override string ToString()
    {
        return $"Name: {Name}";
    }
}