namespace JsonFunctions;

public class Product
{
    public int Id { get; set; }

    public required string Name { get; set; }

    // Raw JSON documents whose shape varies from row to row.
    public required string JsonData { get; set; }

    // A primitive collection, stored as a JSON array.
    public List<string> Tags { get; set; } = [];
}
