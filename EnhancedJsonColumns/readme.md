# Enhanced JSON Columns

Support for JSON columns was introduced in EF Core 7. You could query and update JSON columns. But there were some limitations. EF Core 8 adds some more advanced JSON capabilities.  We can now query JSON collections of complex objects.

## Use Cases

- Use JSON columns to store complex objects/arrays
- Removes previous limitations of JSON queries

## EF Core 11: owned JSON collections without a key

EF Core 11 marks owned entity collections mapped to JSON without a key as obsolete and logs `OwnedEntityMappedToJsonCollectionWarning`. The breaking-changes docs suggest configuring a key on the owned type (`HasKey`), but in EF Core 11.0.0 that throws "Explicitly configured primary keys are not supported on entities stored in JSON collections". So this sample keeps its owned types and ignores the warning (`ConfigureWarnings(w => w.Ignore(RelationalEventId.OwnedEntityMappedToJsonCollectionWarning))`). The recommended long-term move is a complex collection, `ComplexCollection(x => x.Items, b => b.ToJson())`, shown in the [ComplexTypes11](../ComplexTypes11) sample.

## Resources

- [EF Core Docs | Enhanced JSON Columns](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-8.0/whatsnew#enhancements-to-json-column-mapping)
