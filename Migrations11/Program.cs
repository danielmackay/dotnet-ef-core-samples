using Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Migrations11.Audit;
using Migrations11.Catalog;

Console.WriteLine("Migrations 11 Sample");

// Both contexts live in this project. Each has its own database and its own migrations folder.
var sql = new SqlCapture();
using var catalog = new CatalogDbContext(sql);
using var audit = new AuditDbContext();

catalog.Database.EnsureDeleted();
audit.Database.EnsureDeleted();
catalog.Database.Migrate();
audit.Database.Migrate();

Console.WriteLine();
Console.WriteLine("1. Every committed migration was applied");
var catalogMigrations = catalog.Database.GetMigrations().ToList();
var auditMigrations = audit.Database.GetMigrations().ToList();
Console.WriteLine($"Catalog: {string.Join(", ", catalogMigrations)}");
Console.WriteLine($"Audit:   {string.Join(", ", auditMigrations)}");
Check.Equal(catalogMigrations.Count, catalog.Database.GetAppliedMigrations().Count(), "all catalog migrations applied");
Check.Equal(auditMigrations.Count, audit.Database.GetAppliedMigrations().Count(), "all audit migrations applied");

Console.WriteLine();
Console.WriteLine("2. The model snapshot records the latest migration ID");
var snapshotLatestId = LatestMigrationIdInSnapshot(catalog);
Console.WriteLine($"Snapshot says: {snapshotLatestId}");
Check.Equal(catalogMigrations[^1], snapshotLatestId, "catalog snapshot holds the latest catalog migration ID");
Check.Equal(auditMigrations[^1], LatestMigrationIdInSnapshot(audit), "audit snapshot holds the latest audit migration ID");

Console.WriteLine();
Console.WriteLine("3. ExcludeForeignKeyFromMigrations(): EF keeps the relationship, the database has no constraint");
var productToSupplier = catalog.Model.FindEntityType(typeof(Product))?.GetForeignKeys().SingleOrDefault();
Check.That(productToSupplier is not null, "the model still has the Product -> Supplier relationship");
// The flag only matters to migrations, so it lives in the design-time model, not the runtime one.
var designTimeFk = catalog.GetService<IDesignTimeModel>().Model
    .FindEntityType(typeof(Product))?.GetForeignKeys().SingleOrDefault();
Check.That(designTimeFk?.IsExcludedFromMigrations() == true, "the relationship is marked as excluded from migrations");

var foreignKeys = catalog.Database
    .SqlQuery<string>($"SELECT name AS [Value] FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID(N'dbo.Products')")
    .ToList();
Console.WriteLine($"Foreign keys on dbo.Products: {(foreignKeys.Count == 0 ? "(none)" : string.Join(", ", foreignKeys))}");
Check.Equal(0, foreignKeys.Count, "sys.foreign_keys has no constraint on Products");

var indexes = catalog.Database
    .SqlQuery<string>($"SELECT name AS [Value] FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Products') AND name IS NOT NULL")
    .ToList();
Console.WriteLine($"Indexes on dbo.Products: {string.Join(", ", indexes)}");
Check.That(indexes.Contains("IX_Products_SupplierId"), "sys.indexes still has IX_Products_SupplierId");

Console.WriteLine();
Console.WriteLine("4. The relationship still works in queries");
var acme = new Supplier { Name = "Acme" };
catalog.Suppliers.Add(acme);
catalog.Products.AddRange(
    new Product { Name = "Anvil", Price = 99.95m, Rating = 4, Supplier = acme },
    new Product { Name = "Rocket Skates", Price = 149.00m, Rating = 2, Supplier = acme });
catalog.SaveChanges();
catalog.ChangeTracker.Clear();

sql.Clear();
var suppliers = catalog.Suppliers.Include(s => s.Products).ToList();
Console.WriteLine(sql.Last);
foreach (var supplier in suppliers)
    Console.WriteLine($"{supplier.Name}: {string.Join(", ", supplier.Products.Select(p => p.Name))}");
Check.That(sql.Last.Contains("JOIN [Products]", StringComparison.Ordinal), "Include joins Products");
Check.Equal(2, suppliers.SingleOrDefault(s => s.Name == "Acme")?.Products.Count ?? 0, "Include loads both of Acme's products");

Console.WriteLine();
Console.WriteLine("5. Without the constraint, the database accepts a product whose supplier hasn't arrived yet");
// This is the point of excluding the FK: e.g. suppliers are synced in later by another system.
catalog.Products.Add(new Product { Name = "Mystery Box", Price = 5m, SupplierId = 999 });
catalog.SaveChanges();
catalog.ChangeTracker.Clear();
var orphan = catalog.Products.SingleOrDefault(p => p.Name == "Mystery Box");
Check.Equal(999, orphan?.SupplierId, "the product saved with SupplierId 999");

// Watch out: EF still believes the relationship is required (SupplierId isn't nullable), so
// Include uses an INNER JOIN and silently drops products whose supplier is missing.
sql.Clear();
var withSupplier = catalog.Products.Include(p => p.Supplier).ToList().ConvertAll(p => p.Name);
Console.WriteLine(sql.Last);
Console.WriteLine($"Products returned with Include: {string.Join(", ", withSupplier)}");
Check.That(sql.Last.Contains("INNER JOIN [Suppliers]", StringComparison.Ordinal), "Include on the required navigation uses INNER JOIN");
Check.That(!withSupplier.Contains("Mystery Box"), "so the orphaned product is filtered out of the Include query");

Console.WriteLine();
Console.WriteLine("6. The second context works side by side");
audit.AuditEntries.Add(new AuditEntry { Message = "Catalog seeded", CreatedAt = DateTime.UtcNow });
audit.SaveChanges();
Check.Equal(1, audit.AuditEntries.Count(), "the audit context saved and read an entry");

Console.WriteLine();
Check.Summary();

Console.ReadLine();

static string? LatestMigrationIdInSnapshot(DbContext db) =>
    db.GetService<IMigrationsAssembly>().ModelSnapshot?.LastMigrationId;
