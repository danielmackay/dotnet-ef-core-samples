namespace PrimitiveCollections;

public class Product
{
    public int Id { get; private set; }
    public required string Name { get; init; }

    public List<Color> Colors { get; init; } = [];

    public override string ToString()
    {
        return $"Id: {Id}, Name: {Name}, Colors: {string.Join(", ", Colors)}";
    }
}

public enum Color
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