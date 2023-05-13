using TablePerType.Complex;
using TablePerType.Simple;

Console.WriteLine("Table Per Type Sample");

using var simpleDb = new SimpleDbContext();
simpleDb.Database.EnsureDeleted();
simpleDb.Database.EnsureCreated();

using var complexDb = new ComplexDbContext();
complexDb.Database.EnsureDeleted();
complexDb.Database.EnsureCreated();

Console.ReadLine();