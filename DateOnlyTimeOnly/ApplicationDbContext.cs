﻿using Microsoft.EntityFrameworkCore;

namespace DateOnlyTimeOnly;

public class ApplicationDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseSqlServer(
                @"Server=(localdb)\mssqllocaldb;Database=DateOnlyTimeOnly;Trusted_Connection=True");

        //optionsBuilder.LogTo(Console.WriteLine, new[] { RelationalEventId.CommandExecuted });
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }
}