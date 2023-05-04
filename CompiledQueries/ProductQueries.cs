using Microsoft.EntityFrameworkCore;

namespace CompiledQueries;

internal class ProductQueries
{
    public static readonly Func<ApplicationDbContext, string, Product?> GetFirstOrDefault =
        EF.CompileQuery((ApplicationDbContext dbContext, string name) =>
            dbContext.Set<Product>().FirstOrDefault(p => p.Name == name));

    public static readonly Func<ApplicationDbContext, List<Product>> GetAll =
        EF.CompileQuery((ApplicationDbContext dbContext) =>
                dbContext.Set<Product>().ToList());
}
