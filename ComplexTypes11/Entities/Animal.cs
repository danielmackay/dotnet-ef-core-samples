namespace ComplexTypes11.Entities;

// TPT hierarchy: Animals, Dogs and Cats each get their own table.
public abstract class Animal
{
    public int Id { get; set; }
    public required string Name { get; set; }

    // A complex type on the base type of a TPT hierarchy: EF 10 rejected this, EF 11 maps it to columns in the Animals table.
    public required AnimalDetails Details { get; set; }
}

public class Dog : Animal
{
    public required string Breed { get; set; }

    // Mapped to a JSON column on the Dogs table (a TPT derived type).
    public DogExtras? Extras { get; set; }
}

public class Cat : Animal
{
    public bool IsIndoor { get; set; }
}

public class AnimalDetails
{
    public DateOnly BirthDate { get; set; }
    public string? Veterinarian { get; set; }
}

public class DogExtras
{
    public required string Collar { get; set; }
    public int Tricks { get; set; }
}
