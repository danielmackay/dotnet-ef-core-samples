# Json Columns

Store JSON in a column in a database.

## Use Cases

- Dynamic data structures (structure cannot be represented or would be too complex for a relational database)
- Frequently changing data structures (at the start of an uncertain project)
- Raw data storage (e.g. imports or API calls from external systems)

## Restrictions

There are some restrictions as to the SQL operations that can be performed on the JSON column:

- Can't project via `SelectMany()` 

This will improve in EF Core 8.0.