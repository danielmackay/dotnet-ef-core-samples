﻿using Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace TablePerType.Simple;

public class SimpleDbContext : DbContext
{
    public DbSet<Animal> Animals { get; set; }

    public DbSet<Pet> Pets { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseSqlServer(DbConnectionFactory.Create("SimpleTpt"));

        optionsBuilder.LogTo(Console.WriteLine, new[] { RelationalEventId.CommandExecuted });
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Animal>().UseTptMappingStrategy();
    }
}