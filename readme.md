# dotnet-ef-core-samples

[![.NET](https://github.com/danielmackay/dotnet-ef-core-samples/actions/workflows/dotnet.yml/badge.svg)](https://github.com/danielmackay/dotnet-ef-core-samples/actions/workflows/dotnet.yml)

A collection of the latest features of EF Core.  These are the more interesting (IMHO), but less commonly used features.  The samples have been created to help learn and explore EF Core.

## Usage

1. Run the AppHost to start SQL Server
2. Run any sample

Each sample is a self-contained console application that can be run independently to explore and learn.

To run every sample and see each one's exit code, start the AppHost and run `./run-samples.sh` (or `./run-samples.sh <Sample> ...` for a few). The script exits with a non-zero code if any sample fails.

The repo targets .NET 11 and EF Core 11. The `dotnet-ef` CLI is pinned in the local tool manifest, so run `dotnet tool restore` once and then `dotnet dotnet-ef ...` (or `dotnet ef ...`).

## All Features

- [Backing Fields](BackingFields)
- [Buffering and Streaming](BufferingAndStreaming)
- [Bulk Deletes](BulkDeletes)
- [Bulk Updates](BulkUpdates)
- [Compiled Models](CompiledModels)
- [Compiled Queries](CompiledQueries)
- [Complex Types](ComplexTypes)
- [DateOnly and TimeOnly](DateOnlyTimeOnly)
- [DbContext Pooling](DbContextPooling)
- [Enhanced Bulk Update and Delete](EnhancedBulkUpdateAndDelete)
- [Enhanced JSON Columns](EnhancedJsonColumns)
- [Hierarchy IDs](HierarchyIds)
- [JSON Columns](JsonColumns)
- [Keyset Pagination](KeysetPagination)
- [Migration Bundles](MigrationBundles)
- [Migrations 11](Migrations11)
- [Owned Entities](OwnedEntities)
- [Primitive Collections](PrimitiveCollections)
- [Query Filters](QueryFilters)
- [Sentinel Values](SentinelValues)
- [Sequences](Sequences)
- [Shadow Properties](ShadowProperties)
- [Smart Enums](SmartEnums)
- [Split Queries](SplitQueries)
- [Table per Concrete Type](TablePerConcreteType)
- [Table per Hierarchy](TablePerHierarchy)
- [Table per Type](TablePerType)
- [Tags](Tags)
- [Temporal Tables](TemporalTables)
- [Unmapped Types](UnmappedTypes)
- [Value Converters](ValueConverters)

## Features by EF Core Version

Grouped by the EF Core version that introduced the feature.

### EF Core 11.0

The EF Core 11 samples check their own results: each prints a `PASS`/`FAIL` line per check and exits with a non-zero code if any check fails.

- [Migrations 11](Migrations11) (`ExcludeForeignKeyFromMigrations()`, latest migration ID in the snapshot, `dotnet ef` CLI changes)
- [Temporal Tables](TemporalTables) (period columns mapped to CLR properties)

### EF Core 8.0

- [Complex Types](ComplexTypes)
- [DateOnly and TimeOnly](DateOnlyTimeOnly)
- [Enhanced Bulk Update and Delete](EnhancedBulkUpdateAndDelete)
- [Enhanced JSON Columns](EnhancedJsonColumns)
- [Hierarchy IDs](HierarchyIds)
- [Primitive Collections](PrimitiveCollections)
- [Sentinel Values](SentinelValues)
- [Unmapped Types](UnmappedTypes)

### EF Core 7.0

- [Bulk Deletes](BulkDeletes)
- [Bulk Updates](BulkUpdates)
- [JSON Columns](JsonColumns)
- [Table per Concrete Type](TablePerConcreteType)

### EF Core 6.0

- [Compiled Models](CompiledModels)
- [Migration Bundles](MigrationBundles)
- [Temporal Tables](TemporalTables)

### EF Core 5.0

- [Split Queries](SplitQueries)
- [Table per Type](TablePerType)

### EF Core 2.x

- [Compiled Queries](CompiledQueries) (2.0)
- [DbContext Pooling](DbContextPooling) (2.0)
- [Owned Entities](OwnedEntities) (2.0)
- [Query Filters](QueryFilters) (2.0)
- [Value Converters](ValueConverters) (2.1)
- [Tags](Tags) (2.2)

### EF Core 1.x

- [Backing Fields](BackingFields) (1.1)
- [Buffering and Streaming](BufferingAndStreaming) (1.0)
- [Sequences](Sequences) (1.0)
- [Shadow Properties](ShadowProperties) (1.0)
- [Table per Hierarchy](TablePerHierarchy) (1.0)

### Patterns (any version)

These samples show techniques built on top of EF Core rather than a single EF Core feature.

- [Keyset Pagination](KeysetPagination)
- [Smart Enums](SmartEnums) (uses `Ardalis.SmartEnum.EFCore` and value converters)
