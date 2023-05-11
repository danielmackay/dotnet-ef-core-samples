namespace TablePerConcreateType.Complex;

public abstract class Animal
{
    public int Id { get; set; }
    public required string Name { get; set; }
}

public abstract class Pet : Animal
{
    public string? Vet { get; set; }
}

public class FarmAnimal : Animal
{
    public string? Farm { get; set; }
}

public class Cat : Pet
{
    public string? EducationLevel { get; set; }
}

public class Dog : Pet
{
    public string? FavoriteToy { get; set; }
}
