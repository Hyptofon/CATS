using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Domain.Products;
using LanguageExt;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class ProductTypeRepository(ApplicationDbContext context) 
    : BaseRepository<ProductType>(context), IProductTypeRepository, IProductTypeQueries
{
    public async Task<Option<ProductType>> GetByNameAsync(
        string name, 
        CancellationToken cancellationToken)
    {
        var entity = await context.ProductTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Name.ToLower() == name.ToLower() && !x.IsDeleted, 
                cancellationToken);

        return entity ?? Option<ProductType>.None;
    }

    public async Task<Option<ProductType>> GetByIdAsync(
        int id, 
        CancellationToken cancellationToken)
    {
        var entity = await context.ProductTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        return entity ?? Option<ProductType>.None;
    }

    public async Task<IReadOnlyList<ProductType>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await context.ProductTypes
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }
}