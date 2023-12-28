# Sentinel Values and Database Defaults

EF Core can configure SQL Server to use Database defaults.  For this to work, EF needs to know when NOT to send a value to the DB so that the DB can use the default value.  It does this by using the `default` value of the .NET CLR type  This works well for reference types, but not value types.

However, in some cases the CLR default value is a value valid to insert.  For example, a default makes sense when creating a record.  But what if you want to create a record with the default CLR value?  Previously you couldn't.  This is where Sentinel Values come in.

## Use Cases

- Inserting rows with default CLR values when the DB has a default value
- Correct EF Core behavior when using boolean `default` and `enum` default values
- Overriding defaults for other value types such as `int`, `DateTime`,  etc.

## Resources

- [EF Core Docs | Sentinel Values and Database Defaults](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-8.0/whatsnew#database-defaults-for-enums)
