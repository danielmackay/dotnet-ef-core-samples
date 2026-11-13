using Common;
using ComplexTypes11;
using ComplexTypes11.Entities;
using Microsoft.EntityFrameworkCore;

Console.WriteLine("Complex Types (EF Core 11) Sample");

var sql = new SqlCapture();
using (var db = new ApplicationDbContext(sql))
{
    db.Database.EnsureDeleted();
    db.Database.Migrate();

    db.Animals.AddRange(
        new Dog
        {
            Name = "Rex",
            Breed = "Labrador",
            Details = new AnimalDetails { BirthDate = new DateOnly(2020, 3, 14), Veterinarian = "Dr Smith" },
            Extras = new DogExtras { Collar = "Red", Tricks = 3 }
        },
        new Cat
        {
            Name = "Tom",
            IsIndoor = true,
            Details = new AnimalDetails { BirthDate = new DateOnly(2019, 7, 1) }
        });

    db.Vehicles.AddRange(
        new Car
        {
            Doors = 5,
            Spec = new VehicleSpec { Make = "Volkswagen", Model = "Golf" },
            Features = new CarFeatures { Colour = "Blue", Sunroof = true }
        },
        new Truck
        {
            Axles = 3,
            Spec = new VehicleSpec { Make = "MAN", Model = "TGX" }
        });

    db.Customers.AddRange(
        new Customer
        {
            Number = new CustomerNumber { Value = "C-001" },
            Name = "Alice",
            Address = new Address { Street = "1 George St", PostalCode = "2000" },
            Contact = new ContactDetails { Email = "alice@example.com", Address = new ContactAddress { City = "Sydney" } }
        },
        new Customer
        {
            Number = new CustomerNumber { Value = "C-002" },
            Name = "Bob",
            Address = new Address { Street = "5 Collins St", PostalCode = "3000" },
            Contact = new ContactDetails { Email = "bob@example.com", Address = new ContactAddress { City = "Melbourne" } }
        });

    db.Orders.AddRange(
        new Order { Lines = [new OrderLine { Sku = "KB-01", Quantity = 1 }, new OrderLine { Sku = "MS-02", Quantity = 2 }] },
        new Order { Lines = [new OrderLine { Sku = "MN-03", Quantity = 1 }] });
    db.SaveChanges();
}

using (var db = new ApplicationDbContext(sql))
{
    Console.WriteLine();
    Console.WriteLine("1. TPT hierarchy: complex type on the base type, JSON column on a derived type");

    Check.That(TableExists(db, "Animals") && TableExists(db, "Dogs") && TableExists(db, "Cats"),
        "TPT creates Animals, Dogs and Cats tables");

    sql.Clear();
    var dog = db.Animals.OfType<Dog>().Single();
    Console.WriteLine(sql.Last);
    Check.Equal("Labrador", dog.Breed, "Dog breed round-trips");
    Check.Equal(new DateOnly(2020, 3, 14), dog.Details.BirthDate, "Dog Details.BirthDate (complex type, Animals table) round-trips");
    Check.Equal("Dr Smith", dog.Details.Veterinarian, "Dog Details.Veterinarian round-trips");
    Check.Equal("Red", dog.Extras?.Collar, "Dog Extras.Collar (JSON column, Dogs table) round-trips");
    Check.Equal(3, dog.Extras?.Tricks, "Dog Extras.Tricks round-trips");

    var cat = db.Animals.OfType<Cat>().Single();
    Check.That(cat.IsIndoor, "Cat IsIndoor round-trips");
    Check.Equal(new DateOnly(2019, 7, 1), cat.Details.BirthDate, "Cat Details.BirthDate round-trips");
    Check.Equal<string?>(null, cat.Details.Veterinarian, "Cat Details.Veterinarian stays null");

    Console.WriteLine();
    Console.WriteLine("2. TPC hierarchy: complex type on the base type, JSON column on a concrete type");

    Check.That(TableExists(db, "Cars") && TableExists(db, "Trucks") && !TableExists(db, "Vehicles"),
        "TPC creates Cars and Trucks tables and no Vehicles table");

    sql.Clear();
    var car = db.Vehicles.OfType<Car>().Single();
    Console.WriteLine(sql.Last);
    Check.Equal(5, car.Doors, "Car doors round-trip");
    Check.Equal("Volkswagen", car.Spec.Make, "Car Spec.Make (complex type, Cars table) round-trips");
    Check.Equal("Golf", car.Spec.Model, "Car Spec.Model round-trips");
    Check.Equal("Blue", car.Features.Colour, "Car Features.Colour (JSON column, Cars table) round-trips");
    Check.That(car.Features.Sunroof, "Car Features.Sunroof round-trips");

    var truck = db.Vehicles.OfType<Truck>().Single();
    Check.Equal(3, truck.Axles, "Truck axles round-trip");
    Check.Equal("MAN", truck.Spec.Make, "Truck Spec.Make (complex type, Trucks table) round-trips");

    Console.WriteLine();
    Console.WriteLine("3. Nested complex property configured straight from the Property(...) lambda");

    // Spec_Make lives in both TPC tables, so the max length shows up in each.
    Check.Equal(50, MaxLength(db, "Cars", "Spec_Make"), "Cars.Spec_Make is nvarchar(50)");
    Check.Equal(50, MaxLength(db, "Trucks", "Spec_Make"), "Trucks.Spec_Make is nvarchar(50)");

    Console.WriteLine();
    Console.WriteLine("4. Keys and indexes on complex-type properties");

    Check.Equal("Number_Value", PrimaryKeyColumns(db, "Customers"), "Customers primary key is the Number.Value complex property");
    Check.Equal("Address_PostalCode", IndexColumns(db, "Customers", "IX_Customers_Address_PostalCode"),
        "index by lambda: IX_Customers_Address_PostalCode on Address_PostalCode");
    Check.Equal("Address_Street", IndexColumns(db, "Customers", "IX_Customers_Address_Street"),
        "index by name: IX_Customers_Address_Street on Address_Street");

    sql.Clear();
    var alice = db.Customers.Single(c => c.Number.Value == "C-001");
    Console.WriteLine(sql.Last);
    Check.Equal("Alice", alice.Name, "lookup by complex key returns Alice");

    Console.WriteLine();
    Console.WriteLine("5. SQL Server JSON index on a path inside a json-mapped complex type");

    Check.Equal("json", ColumnType(db, "Customers", "Contact"), "Customers.Contact is the native json type");
    Check.Equal("JSON", IndexType(db, "Customers", "IX_Customers_Contact_Address_City"), "IX_Customers_Contact_Address_City is a JSON index");
    Check.Equal("$.Address.City", JsonIndexPath(db, "Customers"), "the JSON index covers $.Address.City");

    sql.Clear();
    var sydney = db.Customers.Where(c => c.Contact.Address.City == "Sydney").Select(c => c.Name).ToList();
    Console.WriteLine(sql.Last);
    Check.That(sydney.SequenceEqual(["Alice"]), "filter on Contact.Address.City returns Alice");

    Console.WriteLine();
    Console.WriteLine("6. Index on a [] path into a JSON complex collection");

    sql.Clear();
    var withMouse = db.Orders.Where(o => o.Lines.Any(l => l.Sku == "MS-02")).ToList();
    Console.WriteLine(sql.Last);
    Check.Equal(1, withMouse.Count, "one order has an MS-02 line");
    Check.Equal(2, withMouse.FirstOrDefault()?.Lines.Count, "that order's two lines round-trip");

    // SQL Server 2025 rejects $[*] in CREATE JSON INDEX (Msg 13683), so this model is only scripted, never applied.
    using var collectionIndexDb = new CollectionIndexContext();
    var script = collectionIndexDb.Database.GenerateCreateScript();
    var jsonIndexLine = script.Split('\n').FirstOrDefault(l => l.Contains("CREATE JSON INDEX", StringComparison.Ordinal))?.Trim();
    Console.WriteLine(jsonIndexLine);
    Check.That(jsonIndexLine?.Contains("FOR (N'$[*].Sku')", StringComparison.Ordinal) == true,
        "HasIndex(\"Lines[].Sku\") scripts a JSON index on $[*].Sku");
}

Check.Summary();

Console.ReadLine();

static bool TableExists(ApplicationDbContext db, string table) =>
    db.Database
        .SqlQuery<int>($"SELECT COUNT(*) AS [Value] FROM sys.tables WHERE name = {table}")
        .Single() == 1;

// CHARACTER_MAXIMUM_LENGTH is in characters (sys.columns.max_length is in bytes, so 100 for nvarchar(50)); -1 means max.
static int? MaxLength(ApplicationDbContext db, string table, string column) =>
    db.Database
        .SqlQuery<int?>($"""
            SELECT CHARACTER_MAXIMUM_LENGTH AS [Value]
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = {table} AND COLUMN_NAME = {column}
            """)
        .Single();

static string ColumnType(ApplicationDbContext db, string table, string column) =>
    db.Database
        .SqlQuery<string>($"""
            SELECT DATA_TYPE AS [Value]
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = {table} AND COLUMN_NAME = {column}
            """)
        .Single();

static string PrimaryKeyColumns(ApplicationDbContext db, string table) =>
    string.Join(", ", db.Database
        .SqlQuery<string>($"""
            SELECT c.name AS [Value]
            FROM sys.indexes i
            JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
            JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
            WHERE i.object_id = OBJECT_ID({table}) AND i.is_primary_key = 1
            ORDER BY ic.key_ordinal
            """)
        .ToList());

static string IndexColumns(ApplicationDbContext db, string table, string index) =>
    string.Join(", ", db.Database
        .SqlQuery<string>($"""
            SELECT c.name AS [Value]
            FROM sys.indexes i
            JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
            JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
            WHERE i.object_id = OBJECT_ID({table}) AND i.name = {index}
            ORDER BY ic.key_ordinal
            """)
        .ToList());

static string? IndexType(ApplicationDbContext db, string table, string index) =>
    db.Database
        .SqlQuery<string>($"SELECT type_desc AS [Value] FROM sys.indexes WHERE object_id = OBJECT_ID({table}) AND name = {index}")
        .SingleOrDefault();

static string? JsonIndexPath(ApplicationDbContext db, string table) =>
    db.Database
        .SqlQuery<string>($"SELECT path AS [Value] FROM sys.json_index_paths WHERE object_id = OBJECT_ID({table})")
        .SingleOrDefault();
