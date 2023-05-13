# Unmapped Types

Unmapped Types is a new feature in EF Core 8.0 that allows you to execute raw SQL queries and commands that return types that are not mapped to entities that EF knows about.

## Use Cases

- Simple query and command scenarios that don't require entities
- Provide 'Dapper-like' functionality for simple queries and commands
- Increased performance for queries and commands that don't require entities
