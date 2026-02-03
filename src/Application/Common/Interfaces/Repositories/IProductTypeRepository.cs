using Domain.Products;
using LanguageExt;

namespace Application.Common.Interfaces.Repositories;

public interface IProductTypeRepository
{
    void Add(ProductType productType);
    void Update(ProductType productType);
    void Delete(ProductType productType);
    Task<Option<ProductType>> GetByNameAsync(string name, CancellationToken cancellationToken);
    Task<Option<ProductType>> GetByIdAsync(int id, CancellationToken cancellationToken);
}