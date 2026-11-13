namespace LinqTranslations11;

public class Customer
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required Email Email { get; set; }
}

// Stored as a plain string through a value converter. The implicit conversion lets queries
// treat it as a string, which is what used to produce the redundant CAST.
public readonly record struct Email(string Value)
{
    public static implicit operator string(Email email) => email.Value;
}

// No navigation to Customer on purpose: some orders point at customers that don't exist,
// which is exactly what a FULL JOIN surfaces.
public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public decimal Total { get; set; }
}

public class Blog
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int Rating { get; set; }
    public int OwnerId { get; set; }
    public Person? Owner { get; set; }
    public List<Post> Posts { get; set; } = [];
}

public class Person
{
    public int Id { get; set; }
    public required string Name { get; set; }
}

public class Post
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Category { get; set; }
    public int Likes { get; set; }
    public int BlogId { get; set; }
    public Blog? Blog { get; set; }
}
