namespace ComplexTypes11.Entities;

// TPC hierarchy: no Vehicles table, Cars and Trucks each hold every column, base ones included.
public abstract class Vehicle
{
    public int Id { get; set; }

    // A complex type on the base type of a TPC hierarchy: its columns are repeated in the Cars and Trucks tables.
    public required VehicleSpec Spec { get; set; }
}

public class Car : Vehicle
{
    public int Doors { get; set; }

    // Mapped to a JSON column on the Cars table (a TPC concrete type).
    public required CarFeatures Features { get; set; }
}

public class Truck : Vehicle
{
    public int Axles { get; set; }
}

public class VehicleSpec
{
    public required string Make { get; set; }
    public required string Model { get; set; }
}

public class CarFeatures
{
    public required string Colour { get; set; }
    public bool Sunroof { get; set; }
}
