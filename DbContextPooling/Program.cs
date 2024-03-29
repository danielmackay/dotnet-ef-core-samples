﻿using Common;
using DbContextPooling;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("DbContext Pooling Sample");

Console.WriteLine("Pooling WITHOUT Dependency Injection");

var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseSqlServer(DbConnectionFactory.Create("DbContextPooling"));

var factory = new PooledDbContextFactory<ApplicationDbContext>(options.Options);

var db = factory.CreateDbContext();
db.Database.EnsureDeleted();
db.Database.EnsureCreated();

Console.WriteLine("Pooling WITH Dependency Injection");

var services = new ServiceCollection();
services.AddDbContextPool<ApplicationDbContext>(
    o => o.UseSqlServer(DbConnectionFactory.Create("DbContextPooling")));

var sp = services.BuildServiceProvider();

using var db2 = sp.GetRequiredService<ApplicationDbContext>();
db2.Database.EnsureDeleted();
db2.Database.EnsureCreated();

Console.ReadLine();