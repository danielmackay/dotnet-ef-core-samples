# Keyset Pagination

Offset paging is the most common form of paging.  This is done by using `Skip()` and `Take()` LINQ methods. It is also the most inefficient. This is because the offset is the number of records to skip. This means that the database must scan all the records to find the offset. This is not a problem for small tables, but as the table grows, the performance will degrade. This is because the database must scan more and more records to find the offset. This is why offset paging is not recommended for large tables.

Keyset paging is a more efficient form of paging. This is because the offset is the value of the last record on the previous page. This means that the database can use an index to find the offset. This is much more efficient than scanning all the records. This is why keyset paging is recommended for large tables.

## Use Cases

- Performant paging for large tables

## Resources

- [EF Core Docs | Pagination](https://learn.microsoft.com/en-us/ef/core/querying/pagination)
