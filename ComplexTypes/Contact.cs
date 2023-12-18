namespace ComplexTypes;

public class Contact
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public required Address Address { get; init; }

    public override string ToString()
    {
        return $"ID:{Id}, Name:{Name}, Address:{Address}";
    }
}

public class Address
{
    public required string Street { get; init; }
    public required string City { get; init; }
    public required string PostCode { get; init; }

    public override string ToString()
    {
        return $"Street:{Street}, City:{City}, PostCode:{PostCode}";
    }
}