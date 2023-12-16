﻿namespace TemporalTables;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }

    public override string ToString()
    {
        return $"Id: {Id}, Name: {Name}, Price: {Price}";
    }
}