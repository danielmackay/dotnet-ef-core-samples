using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("DbContext Pooling Sample");

Console.WriteLine("Pooling WITHOUT Dependency Injection");

var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=DbContextPooling;Trusted_Connection=True");

var factory = new PooledDbContextFactory<ApplicationDbContext>(options.Options);

var db = factory.CreateDbContext();
db.Database.EnsureDeleted();
db.Database.EnsureCreated();

Console.WriteLine("Pooling WITH Dependency Injection");

var services = new ServiceCollection();
services.AddDbContextPool<ApplicationDbContext>(
    o => o.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=DbContextPooling;Trusted_Connection=True"));

var sp = services.BuildServiceProvider();

using var db2 = sp.GetRequiredService<ApplicationDbContext>();
db2.Database.EnsureDeleted();
db2.Database.EnsureCreated();

Console.ReadLine();