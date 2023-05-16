# Compiled Models

EF Core needs to compile the model for the first DB query.  This takes time and can be noticeable for large models.  

Compiled models are created via the dotnet CLI:

```bash
dotnet ef dbcontext optimize
```

You can ensure you've got the latest version of the tools by running:

```bash
dotnet tool update --global dotnet-ef
```

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
