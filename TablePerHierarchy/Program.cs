using TablePerHierarchy.Complex;
using TablePerHierarchy.Simple;

Console.WriteLine("Table Per Hierarchy Sample");

using var simpleDb = new SimpleDbContext();
await simpleDb.Database.EnsureDeletedAsync();
await simpleDb.Database.EnsureCreatedAsync();

using var complexDb = new ComplexDbContext();
await complexDb.Database.EnsureDeletedAsync();
await complexDb.Database.EnsureCreatedAsync();

Console.ReadLine();