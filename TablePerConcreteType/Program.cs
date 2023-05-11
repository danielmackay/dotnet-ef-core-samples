using TablePerConcreateType.Complex;
using TablePerConcreateType.Simple;

Console.WriteLine("Table Per Concrete Type Sample");

using var simpleDb = new SimpleDbContext();
await simpleDb.Database.EnsureDeletedAsync();
await simpleDb.Database.EnsureCreatedAsync();

using var complexDb = new ComplexDbContext();
await complexDb.Database.EnsureDeletedAsync();
await complexDb.Database.EnsureCreatedAsync();

Console.ReadLine();