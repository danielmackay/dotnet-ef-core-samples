# Buffering and Streaming

Buffering and Streaming are two different ways of retrieving data from a database.  Buffering is the default behavior of EF Core.  It means that the entire result set is retrieved from the database and stored in memory.  Streaming means that the result set is retrieved from the database in chunks.  This is useful when the result set is large and you don't want to store it all in memory at once.

## Use Cases

- Use streaming when you need to return a large result set from a query in a memory efficient way.
- Streaming results from an API

## Resources

- [EF Core Docs | Buffering and Streaming](https://learn.microsoft.com/en-us/ef/core/performance/efficient-querying#buffering-and-streaming)
