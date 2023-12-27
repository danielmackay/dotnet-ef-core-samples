namespace EnhancedBulkUpdateAndDelete;

public class Product
{
    public int Id { get; private set; }
    public required string Name { get; init; }

    public required Color Color { get; init; }

    public override string ToString()
    {
        return $"Id: {Id}, Name: {Name}, Color: {Color}";
    }
}

public class Color
{
    public ColorCode Code { get; init; }
    public required string Name { get; init; }
    public int NumInStock { get; init; }

    public override string ToString()
    {
        return $"Code: {Code}, Name: {Name}, NumInStock: {NumInStock}";
    }
}

public enum ColorCode
{
    Red = 1,
    Green,
    Blue,
    Yellow,
    Orange,
    Purple,
    Black,
    White,
    Brown
}