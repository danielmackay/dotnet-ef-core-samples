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

## EF Core 11: owned JSON collections without a key

EF Core 11 marks owned entity collections mapped to JSON without a key as obsolete and logs `OwnedEntityMappedToJsonCollectionWarning`. The breaking-changes docs suggest configuring a key on the owned type (`HasKey`), but in EF Core 11.0.0 that throws "Explicitly configured primary keys are not supported on entities stored in JSON collections". So this sample keeps its owned types and ignores the warning (`ConfigureWarnings(w => w.Ignore(RelationalEventId.OwnedEntityMappedToJsonCollectionWarning))`). The recommended long-term move is a complex collection, `ComplexCollection(x => x.Items, b => b.ToJson())`, shown in the [ComplexTypes11](../ComplexTypes11) sample.
