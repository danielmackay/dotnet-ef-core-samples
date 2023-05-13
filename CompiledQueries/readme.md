# Compiled Queries

EF needs to compile queries before they are executed.  This happens internally, but can be a performance bottleneck if you are executing the same query multiple times.  EF Core 6.0 introduces a new `CompileQuery` method that allows you to pre-compile queries and cache them for later use.

## Use Cases

- Improve performance of frequently executed queries

## Resources

- https://learn.microsoft.com/en-us/ef/core/performance/advanced-performance-topics?tabs=with-di%2Cexpression-api-with-constant#compiled-queries
