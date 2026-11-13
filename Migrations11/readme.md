# Migrations 11

EF Core 11 adds a handful of migrations features. One of them changes your model (`ExcludeForeignKeyFromMigrations()`), one changes the generated files (the latest migration ID in the model snapshot), and the rest are `dotnet ef` CLI improvements.

The sample has two DbContexts in one project, each with its own database and migrations folder:

| Context | Database | Migrations |
|---|---|---|
| `CatalogDbContext` (suppliers and products) | `Migrations11` | `Catalog/Migrations` |
| `AuditDbContext` (audit entries) | `Migrations11Audit` | `Audit/Migrations` |

Running the sample drops both databases, applies the committed migrations at runtime, and checks the results. It exits with a non-zero code if any check fails.

## Excluding a foreign key constraint from migrations

Sometimes you want EF to know about a relationship without the database enforcing it, for example when the principal rows are synced in later by another system, or when you map a legacy database. EF Core 11 adds `ExcludeForeignKeyFromMigrations()`:

```csharp
modelBuilder.Entity<Product>()
    .HasOne(p => p.Supplier)
    .WithMany(s => s.Products)
    .HasForeignKey(p => p.SupplierId)
    .ExcludeForeignKeyFromMigrations();
```

The generated migration creates the `Products` table with no `table.ForeignKey(...)`, but it still creates the index on the foreign key column:

```csharp
migrationBuilder.CreateIndex(
    name: "IX_Products_SupplierId",
    table: "Products",
    column: "SupplierId");
```

EF still treats it as a relationship, so navigations and `Include` work as usual. The sample checks:

- `sys.foreign_keys` has no constraint on `Products`, and `sys.indexes` still has `IX_Products_SupplierId`
- `Include(s => s.Products)` loads each supplier's products
- a product whose supplier doesn't exist yet (`SupplierId = 999`) saves without error

> **Watch out:** EF still thinks the relationship is *required* (`SupplierId` isn't nullable), so `Include(p => p.Supplier)` uses an `INNER JOIN` and silently drops products whose supplier is missing. The sample checks this too. Make the foreign key nullable if orphaned rows should come back from `Include` queries.

The flag only matters to migrations, so it's stored in the design-time model, not the read-optimized runtime model. Read it with `context.GetService<IDesignTimeModel>().Model`; `context.Model` throws.

## The latest migration ID in the model snapshot

The model snapshot now records the ID of the last migration it was generated for:

```csharp
[DbContext(typeof(CatalogDbContext))]
partial class CatalogDbContextModelSnapshot : ModelSnapshot
{
    // If you encounter a merge conflict in the line below, it means you need to
    // discard one of the migration branches and recreate its migrations on top of
    // the other branch. See https://aka.ms/efcore-docs-migrations-conflicts for more info.
    public override string LastMigrationId => "20261113215331_AddProductRating";
```

That line is there to cause a merge conflict on purpose. Before EF Core 11, if two branches each added a migration, git usually merged the snapshot cleanly: each branch changed different lines. The merged code compiled, but the two migrations had been generated without knowing about each other, so they could conflict or leave the snapshot out of step with the model. Now both branches change the same `LastMigrationId` line, so git reports a conflict and you have to deal with it. The fix: keep one branch's migrations, remove the other branch's (`dotnet ef migrations remove`), and add them again on top.

The sample checks that each snapshot's `LastMigrationId` matches that context's last migration.

The generated files have two other changes in EF Core 11: they use file-scoped namespaces, and the migration class name includes the timestamp (`public partial class _20261113215205_InitialCreate : Migration`).

## CLI changes

### Pinning `dotnet-ef` in a tool manifest

The repo's tool manifest ([`dotnet-tools.json`](../dotnet-tools.json) in the repo root) pins `dotnet-ef` 11, so everyone uses the same version. Run `dotnet tool restore` once. After that, `dotnet ef` anywhere in the repo uses the pinned version, not a global install:

```bash
dotnet tool restore
dotnet ef --version   # Entity Framework Core .NET Command-line Tools 11.0.0
```

The project you run `dotnet ef` against needs a reference to `Microsoft.EntityFrameworkCore.Design`. The `Microsoft.EntityFrameworkCore.Tools` package no longer brings it in.

### `.config/dotnet-ef.json`

`dotnet ef` now reads defaults for its options from a `.config/dotnet-ef.json` file. It looks in the current folder and then each parent folder, and relative paths are resolved from the folder that contains `.config`. This sample's [`.config/dotnet-ef.json`](.config/dotnet-ef.json):

```json
{
  "project": ".",
  "startupProject": ".",
  "context": "CatalogDbContext"
}
```

With it, `dotnet ef` works from anywhere inside `Migrations11` (even `Migrations11/Catalog/Migrations`) with no `--project`, `--startup-project` or `--context`. A `--context` on the command line overrides the default. The other supported keys are `framework`, `configuration`, `runtime`, `verbose`, `noColor` and `prefixOutput`.

### Commands

Run these from the `Migrations11` folder, with the AppHost running. Each was run against the AppHost's SQL Server in this order; the output is in the PR.

Start from scratch. `--context "*"` runs the command once for every DbContext in the project:

```bash
dotnet ef database drop --context "*" --force
```

Create a migration and apply it in one step with `database update <name> --add`. Here it creates the audit context's first migration:

```bash
dotnet ef database update InitialCreate --add --context AuditDbContext --output-dir Audit/Migrations
```

> **EF Core 11.0.0 bug:** `--add` only works for a context's *first* migration. For a context that already has migrations it scaffolds against an empty model (a `CreateTable` instead of, say, an `AddColumn`), writes the files, and then fails to apply them with `There is already an object named '...' in the database`. Clean up with `dotnet ef migrations remove --offline` and use `dotnet ef migrations add <name>` followed by `dotnet ef database update`. That's how `AddProductRating` was created.

List migrations. With no `--context`, the config file's `CatalogDbContext` is used:

```bash
dotnet ef migrations list
dotnet ef migrations list --context AuditDbContext
```

> **EF Core 11.0.0 bug:** `dotnet ef migrations list --context "*"` fails with `No DbContext named '*' was found.` It runs for each context and then fails, without printing either list. List each context on its own until that's fixed.

Script every context's migrations, then apply them all:

```bash
dotnet ef migrations script --context "*"
dotnet ef database update --context "*"
```

`migrations remove --offline` removes the last migration without connecting to the database. It doesn't check whether the migration was applied, so only use it on a migration you know hasn't been applied (or after reverting it):

```bash
dotnet ef migrations add Scratch
dotnet ef migrations remove --offline
```

`--connection` points a command at a database other than the one in `OnConfiguring`. `migrations list` and `migrations remove` use it to check what's been applied:

```bash
dotnet ef migrations add Scratch
dotnet ef migrations list --connection 'Server=127.0.0.1,1700;Database=Migrations11;User ID=sa;Password=yourStrong(!)Password;TrustServerCertificate=True'
dotnet ef migrations remove --connection 'Server=127.0.0.1,1700;Database=Migrations11;User ID=sa;Password=yourStrong(!)Password;TrustServerCertificate=True'
```

Clean up:

```bash
dotnet ef database drop --context "*" --force
```

## Resources

- [What's new in EF Core 11: Migrations](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/whatsnew#migrations)
- [Excluding foreign key constraints from migrations](https://learn.microsoft.com/en-us/ef/core/modeling/relationships/foreign-and-principal-keys#excluding-foreign-key-constraints-from-migrations)
- [Resolving migration merge conflicts](https://aka.ms/efcore-docs-migrations-conflicts)
- [EF Core CLI: configuration file](https://learn.microsoft.com/en-us/ef/core/cli/dotnet#configuration-file)
- [EF Core CLI reference](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)
