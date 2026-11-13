using Microsoft.EntityFrameworkCore;

namespace DbContextPooling;

// Pooling requires a single public constructor taking DbContextOptions, because the pool
// creates instances itself and supplies the options it was configured with.
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; }
}
