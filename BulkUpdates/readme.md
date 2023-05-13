# Bulk Updates

Bulk deletes are a new feature in EF Core 6.0.  They allow you to update a large number of entities in a single operation.  This is much more efficient than updating each entity individually.

## Use Cases

- Efficiently update a large number of entities without having to first load them into memory

## Resources

- https://learn.microsoft.com/en-us/ef/core/saving/execute-insert-update-delete