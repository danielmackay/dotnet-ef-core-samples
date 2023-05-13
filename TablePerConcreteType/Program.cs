using TablePerConcreateType.Complex;
using TablePerConcreateType.Simple;

Console.WriteLine("Table Per Concrete Type Sample");

using var simpleDb = new SimpleDbContext();
simpleDb.Database.EnsureDeleted();
simpleDb.Database.EnsureCreated();

using var complexDb = new ComplexDbContext();
complexDb.Database.EnsureDeleted();
complexDb.Database.EnsureCreated();

Console.ReadLine();