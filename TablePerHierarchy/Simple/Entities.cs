namespace TablePerHierarchy.Simple;


public class Animal
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Species { get; set; } = null!;
}

public class Pet : Animal
{
    public string? FavoriteToy { get; set; }
    public string? Vet { get; set; }
}
