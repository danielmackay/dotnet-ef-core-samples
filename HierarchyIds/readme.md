# Hierarchy IDs

Hierarchy IDs are a special data type in SQL Server that allow you to store and query hierarchical data.  This is a great way to store data that has a parent-child relationship, such as a file system, organizational chart, or product categories.

The DB is then able to run queries such as finding all descendants of a node, or finding the common ancestor of two nodes.

NOTE: You will need the CLR enabled on your SQL Server instance for this to work.  The Azure SQL Edge Docker image does not support this.


## Use Cases

- Store and query hierarchical (i.e. tree-like) data

## Resources

- [EF Core Docs | Hierarchy IDs](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-8.0/whatsnew#hierarchyid-in-net-and-ef-core)
