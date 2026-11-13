namespace ComplexTypes11.Entities;

public class Customer
{
    // A value object as the primary key: EF 11 lets the key point at a property inside a complex type.
    public required CustomerNumber Number { get; set; }
    public required string Name { get; set; }

    // Mapped to columns (Address_Street, Address_PostalCode), each indexed.
    public required Address Address { get; set; }

    // Mapped to a native json column, with a SQL Server JSON index on Contact.Address.City.
    public required ContactDetails Contact { get; set; }
}

public class CustomerNumber
{
    public required string Value { get; set; }
}

public class Address
{
    public required string Street { get; set; }
    public required string PostalCode { get; set; }
}

public class ContactDetails
{
    public required string Email { get; set; }
    public required ContactAddress Address { get; set; }
}

public class ContactAddress
{
    public required string City { get; set; }
}
