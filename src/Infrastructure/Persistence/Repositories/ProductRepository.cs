using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Domain.Containers;
using Domain.Products;
using LanguageExt;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class ProductRepository(ApplicationDbContext context) 
    : BaseRepository<Product>(context), IProductRepository, IProductQueries
{
    public async Task<Option<Product>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var product = await context.Products
            .Include(p => p.ProductType)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);

        return product == null ? Option<Product>.None : Option<Product>.Some(product);
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await context.Products
            .Include(p => p.ProductType)
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsProductInUseAsync(int productId, CancellationToken cancellationToken)
    {
        return await context.Containers
            .AnyAsync(c => c.CurrentProductId == productId && 
                          c.Status == ContainerStatus.Full && 
                          !c.IsDeleted, 
                cancellationToken);
    }
}
