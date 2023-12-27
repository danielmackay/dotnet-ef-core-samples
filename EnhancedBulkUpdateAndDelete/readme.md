# Enhanced Bulk Update and Delete

EF Core has always required that Bulk Updates and Deletes be performed on a single table.  However, when using Owned Entities, the data is in a single table, but EF Core still wouldn't allow you to perform a Bulk Update or Delete.

## Use Cases

- Bulk Update and Delete on Owned Entities

## Resources

- [EF Core Docs | Enhanced Bulk Updates and Deletes](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-8.0/whatsnew#better-executeupdate-and-executedelete)
