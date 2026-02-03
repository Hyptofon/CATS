using Domain.Products;
using LanguageExt;

namespace Application.Common.Interfaces.Repositories;

public interface IProductTypeRepository : IBaseRepository<ProductType>
{
    Task<Option<ProductType>> GetByNameAsync(string name, CancellationToken cancellationToken);
    Task<Option<ProductType>> GetByIdAsync(int id, CancellationToken cancellationToken);
}