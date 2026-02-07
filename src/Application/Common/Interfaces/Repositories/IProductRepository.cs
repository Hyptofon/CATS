using Domain.Products;
using LanguageExt;

namespace Application.Common.Interfaces.Repositories;

public interface IProductRepository : IBaseRepository<Product>
{
    Task<Option<Product>> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken);
    Task<bool> IsProductInUseAsync(int productId, CancellationToken cancellationToken);
}
