# Temporal Tables

Temporal tables allow you to track the history of data changes in a table.  This works by creating a shadow table that is used to story a copy of the data for every version. The shadow table is automatically updated when the data in the main table is updated.  The shadow table is not visible to the user, but can be queried using EF Core.
