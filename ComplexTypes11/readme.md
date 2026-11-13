# Complex Types (EF Core 11)

EF Core 8 introduced complex types (see the [Complex Types](../ComplexTypes) sample) and EF Core 10 let you map them to JSON columns. EF Core 11 removes several of the remaining limits:

- complex types work on entities in **TPT** and **TPC** inheritance hierarchies (EF 10 rejected them), including complex types mapped to **JSON columns**
- a nested complex-type property can be configured straight from the `Property(...)` lambda
- **keys and indexes** can point at complex-type properties, by lambda or by name, including `[]` paths into JSON complex collections
- on SQL Server 2025, an index on a path inside a `json` column becomes a **JSON index**

The schema is created by a real migration (`Migrations/`), so you can read the code EF generates. The sample checks its own results and exits non-zero if any check fails.

## Running

Start the AppHost (SQL Server 2025), then:

```bash
dotnet run --project ComplexTypes11
```

To recreate the migration (the project has two DbContexts, so `--context` is needed):

```bash
dotnet tool restore
dotnet dotnet-ef migrations add Initial --project ComplexTypes11 --context ApplicationDbContext
```

Then re-apply the snapshot fix described under [Known issues](#known-issues-in-ef-core-1100).

## Complex types on TPT and TPC hierarchies

```cs
public abstract class Animal
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required AnimalDetails Details { get; set; }   // complex type on a TPT base type
}

public class Dog : Animal
{
    public required string Breed { get; set; }
    public DogExtras? Extras { get; set; }                // JSON column on a TPT derived type
}

public abstract class Vehicle
{
    public int Id { get; set; }
    public required VehicleSpec Spec { get; set; }        // complex type on a TPC base type
}

public class Car : Vehicle
{
    public int Doors { get; set; }
    public required CarFeatures Features { get; set; }    // JSON column on a TPC concrete type
}
```

```cs
modelBuilder.Entity<Animal>().UseTptMappingStrategy().ToTable("Animals");
modelBuilder.Entity<Dog>().ToTable("Dogs");
modelBuilder.Entity<Cat>().ToTable("Cats");
modelBuilder.Entity<Animal>().ComplexProperty(a => a.Details);
modelBuilder.Entity<Dog>().ComplexProperty(d => d.Extras, b => b.ToJson());

modelBuilder.Entity<Vehicle>().UseTpcMappingStrategy();
modelBuilder.Entity<Car>().ToTable("Cars");
modelBuilder.Entity<Truck>().ToTable("Trucks");
modelBuilder.Entity<Vehicle>().ComplexProperty(v => v.Spec);
modelBuilder.Entity<Car>().ComplexProperty(c => c.Features, b => b.ToJson());
```

With TPT, `Details_BirthDate` and `Details_Veterinarian` live in `Animals`, and the `Extras` json column lives in `Dogs`. With TPC there is no `Vehicles` table: `Spec_Make` and `Spec_Model` are repeated in `Cars` and `Trucks`, and `Features` is a json column in `Cars`.

The context uses compatibility level 170 (SQL Server 2025), so `ToJson()` maps to the native `json` type. At the EF 11 default of 160 you get `nvarchar(max)` unless you add `.HasColumnType("json")`.

## Nested properties in the `Property(...)` lambda

```cs
// EF 11
modelBuilder.Entity<Vehicle>().Property(v => v.Spec.Make).HasMaxLength(50);

// Before EF 11
modelBuilder.Entity<Vehicle>().ComplexProperty(v => v.Spec, b => b.Property(s => s.Make).HasMaxLength(50));
```

Both `Cars.Spec_Make` and `Trucks.Spec_Make` become `nvarchar(50)`.

## Keys and indexes on complex-type properties

```cs
modelBuilder.Entity<Customer>().HasKey(c => c.Number.Value);          // key inside a value object
modelBuilder.Entity<Customer>().HasIndex(c => c.Address.PostalCode);  // index by lambda
modelBuilder.Entity<Customer>().HasIndex("Address.Street");           // index by name
```

```sql
CONSTRAINT [PK_Customers] PRIMARY KEY ([Number_Value])
CREATE INDEX [IX_Customers_Address_PostalCode] ON [Customers] ([Address_PostalCode]);
CREATE INDEX [IX_Customers_Address_Street] ON [Customers] ([Address_Street]);
```

## SQL Server JSON indexes

When the complex type is mapped to a `json` column, an index on a path inside it becomes a SQL Server 2025 JSON index:

```cs
modelBuilder.Entity<Customer>().ComplexProperty(c => c.Contact, b => b.ToJson().HasColumnType("json"));
modelBuilder.Entity<Customer>().HasIndex("Contact.Address.City");
```

```sql
CREATE JSON INDEX [IX_Customers_Contact_Address_City] ON [Customers]([Contact]) FOR (N'$.Address.City');
```

The generated migration only shows `CreateIndex(name: "IX_Customers_Contact_Address_City", table: "Customers", column: "Contact")`. The JSON path comes from the target model when the migration SQL is generated (`dotnet dotnet-ef migrations script --project ComplexTypes11 --context ApplicationDbContext` shows it).

SQL Server limits:

- a JSON index needs a `json` column, not `nvarchar(max)`
- only **one JSON index per json column** (Msg 13681), so a second `HasIndex` into `Contact` fails when the migration runs

### `[]` paths into JSON complex collections

`[]` means "every element of the collection":

```cs
modelBuilder.Entity<Order>().ComplexCollection(o => o.Lines, b => b.ToJson().HasColumnType("json"));
modelBuilder.Entity<Order>().HasIndex("Lines[].Sku");
```

```sql
CREATE JSON INDEX [IX_Orders_Lines_Sku] ON [Orders]([Lines]) FOR (N'$[*].Sku');
```

SQL Server 2025 rejects `$[*]` in a JSON index path (`Msg 13683 Invalid JSON paths in JSON index`), so this index lives in a separate `CollectionIndexContext` that the sample only scripts with `GenerateCreateScript()` and never applies. A specific element (`Lines[0].Sku` → `$[0].Sku`) works on SQL Server, and Cosmos DB accepts `[]` paths in its indexes.

## What the sample checks

- Dogs, Cats, Cars and Trucks (complex and JSON-mapped values included) round-trip through save and reload
- `sys.tables` has the TPT tables (`Animals`, `Dogs`, `Cats`) and the TPC tables (`Cars`, `Trucks`, no `Vehicles`)
- `INFORMATION_SCHEMA.COLUMNS` shows `Spec_Make` as length 50 in both TPC tables
- `sys.indexes` / `sys.index_columns` show the primary key on `Number_Value` and the two complex-property indexes
- `sys.indexes` shows `IX_Customers_Contact_Address_City` with `type_desc = 'JSON'`, and `sys.json_index_paths` shows `$.Address.City`
- queries filtering on `Contact.Address.City` and `Lines.Any(l => l.Sku == ...)` return the right rows
- the scripted `[]` index uses `$[*].Sku`

## Known issues in EF Core 11.0.0

- **Complex key in the model snapshot.** `migrations add` writes `b.HasKey("Value")` instead of `b.HasKey("Number.Value")` in the snapshot and the migration's `.Designer.cs`. `Migrate()` then fails with "The property 'Value' cannot be added to the type 'Customer (Dictionary<string, object>)'". Both files in `Migrations/` are hand-edited to `"Number.Value"` (marked with a comment). The migration's `Up` code is correct.
- **`[]` JSON index paths** don't work on SQL Server 2025 (see above).

## Resources

- [What's new in EF Core 11: complex types](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/whatsnew#complex-types)
- [Indexes on complex type properties](https://learn.microsoft.com/en-us/ef/core/modeling/indexes#indexes-on-complex-type-properties)
- [SQL Server JSON indexes in EF Core](https://learn.microsoft.com/en-us/ef/core/providers/sql-server/index#json-indexes)
- [CREATE JSON INDEX (Transact-SQL)](https://learn.microsoft.com/en-us/sql/t-sql/statements/create-json-index-transact-sql)
