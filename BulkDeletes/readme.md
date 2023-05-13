# Bulk Deletes

Bulk deletes are a new feature in EF Core 6.0.  They allow you to delete a large number of entities in a single operation.  This is much more efficient than deleting each entity individually.

## Use Cases

- Efficiently delete a large number of entities without having to first load them into memory

## Resources

- [EF Core Docs | Executing Insert Update Delete](https://learn.microsoft.com/en-us/ef/core/saving/execute-insert-update-delete)
