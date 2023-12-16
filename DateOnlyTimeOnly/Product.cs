﻿namespace DateOnlyTimeOnly;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public bool IsDeleted { get; set; }
    public DateOnly DateCreated { get; set; }
    public TimeOnly TimeCreated { get; set; }

    public override string ToString()
    {
        return $"Id: {Id}, Name: {Name}, IsDeleted: {IsDeleted}, DateCreated: {DateCreated}, TimeCreated: {TimeCreated}";
    }
}