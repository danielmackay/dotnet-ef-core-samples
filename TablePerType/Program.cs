using TablePerType.Complex;
using TablePerType.Simple;

Console.WriteLine("Table Per Type Sample");

using var simpleDb = new SimpleDbContext();
await simpleDb.Database.EnsureDeletedAsync();
await simpleDb.Database.EnsureCreatedAsync();

using var complexDb = new ComplexDbContext();
await complexDb.Database.EnsureDeletedAsync();
await complexDb.Database.EnsureCreatedAsync();

Console.ReadLine();