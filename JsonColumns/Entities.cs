namespace JsonColumns;

public class Contact
{
    public int Id { get; set; }
    public string Name { get; init; } = null!;
    public required Address Address { get; set; } = null!;
    public ICollection<Note>? Notes { get; set; }

    public override string ToString()
    {
        return $"Id: {Id}, Name: {Name}, Address: {Address}, Notes: {Notes}";
    }
}

public class Address
{
    public string? Line1 { get; set; }
    public string? Line2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }

    public override string ToString()
    {
        return $"Line1: {Line1}, Line2: {Line2}, City: {City}, State: {State}";
    }
}

public class Note
{
    public string Text { get; set; } = null!;

    public override string ToString()
    {
        return $"Text: {Text}";
    }
}