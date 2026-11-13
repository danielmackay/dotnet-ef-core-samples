# Compiled Models

EF Core needs to compile the model for the first DB query.  This takes time and can be noticeable for large models.  

Compiled models are created via the dotnet CLI:

```bash
dotnet ef dbcontext optimize
```

The repo pins `dotnet-ef` in its local tool manifest, so restore it once with:

```bash
dotnet tool restore
```

The project being optimized must reference `Microsoft.EntityFrameworkCore.Design`. From EF Core 11 the `Microsoft.EntityFrameworkCore.Tools` package no longer brings it in.

This will generate several classes under a `CompiledModels` folder.  You need to hook this up to your DbOptionsBuilder like so:

```cs
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseSqlServer("{YOUR CONNECTION STRING}")
            .UseModel(ApplicationDbContextModel.Instance);
    }
```

## Use Cases

- Performance increase for models with hundreds or thousands of entities and relationships

## Resources

- [EF Core Docs | Compiled Models](https://learn.microsoft.com/en-us/ef/core/performance/advanced-performance-topics?tabs=with-di%2Cexpression-api-with-constant#compiled-models)
