# DateOnly and TimeOnly

EF Core 8.0 introduces support for the `DateOnly` and `TimeOnly` types.  These are value types that represent a date or time without a time zone.  They are useful for storing dates and times without the overhead of a full `DateTime` object.

## Use Cases

- Storing dates and times without a time zone
