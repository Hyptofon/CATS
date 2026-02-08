using Domain.Products;
using LanguageExt;

namespace Application.Common.Interfaces.Queries;

public interface IProductQueries
{
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken);
    Task<Option<Product>> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Product>> SearchAsync(
        string? searchTerm,
        int? productTypeId,
        CancellationToken cancellationToken);
}
