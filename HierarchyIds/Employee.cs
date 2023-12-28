using Microsoft.EntityFrameworkCore;

namespace HierarchyIds;

public class Employee
{
    public int Id { get; private set; }
    public required HierarchyId Path { get; init; }
    public required string Name { get; init; }

    public override string ToString()
    {
        return $"Id: {Id}, Name: {Name}, Path: {Path}";
    }
}