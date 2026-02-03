using Domain.Products;
using LanguageExt;

namespace Application.Common.Interfaces.Queries;

public interface IProductTypeQueries
{
    Task<Option<ProductType>> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<IReadOnlyList<ProductType>> GetAllAsync(CancellationToken cancellationToken);
}