namespace JsonColumns;

public class Contact
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public Address? Address { get; set; }
    public ICollection<Note>? Notes { get; set; }
}

public class Address
{
    public string? Line1 { get; set; }
    public string? Line2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
}

public class Note
{
    public string Text { get; set; } = null!;
}