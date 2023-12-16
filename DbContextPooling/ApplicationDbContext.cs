using Microsoft.EntityFrameworkCore;

namespace DbContextPooling;

public class ApplicationDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }
}