# Split Queries

Queries with large numbers of joins can erform poorly.  If you have a query with a large number of joins, you can split the query into multiple queries.  For example, if you have a query with 10 joins, you can split the query into 5 queries with 2 joins each.  This will improve performance.

## Use Cases

- Improve performance of queries with large numbers of joins

## Considerations

- Performance of using split queries needs to be measured so you can accurately determine if it is improving performance
- The queries are performed sequentially, which means multiple round trips to the database.  This can be slower than a single query with few joins.