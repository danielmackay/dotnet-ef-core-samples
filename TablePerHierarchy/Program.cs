using TablePerHierarchy.Simple;

Console.WriteLine("Table Per Hierarchy Sample");

using var simpleDb = new SimpleDbContext();
await simpleDb.Database.EnsureDeletedAsync();
await simpleDb.Database.EnsureCreatedAsync();

Console.ReadLine();