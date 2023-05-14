# DbContext Pooling

DbContext pooling allows you to reuse DbContext instances to avoid the cost of creating and disposing of many instances of DbContext. This is especially useful in web applications where you can reuse the same DbContext across multiple requests.

## Use Cases

- Performant DbContext usage in high-performance scenarios

## Considerations

- DbContext will be reused across requests, so you must ensure that it is thread-safe.  This means that you should not store any state in the DbContext that is specific to a single request.

## Resources

- [EF Core Docs | DbContext Pooling](https://learn.microsoft.com/en-us/ef/core/performance/advanced-performance-topics?tabs=with-di%2Cexpression-api-with-constant#dbcontext-pooling)
