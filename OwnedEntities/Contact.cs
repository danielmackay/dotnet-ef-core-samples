public class Contact
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required Address Address { get; set; }

    public override string ToString()
    {
        return $"ID:{Id}, Name:{Name}, Address:{Address}";
    }
}

public class Address
{
    public required string Street { get; set; }
    public required string City { get; set; }
    public required string PostCode { get; set; }

    public override string ToString()
    {
        return $"Street:{Street}, City:{City}, PostCode:{PostCode}";
    }
}
