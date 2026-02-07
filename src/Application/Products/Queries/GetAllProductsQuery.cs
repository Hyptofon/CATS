using Application.Common.Interfaces.Repositories;
using Domain.Products;
using MediatR;

namespace Application.Products.Queries;

public record GetAllProductsQuery :IRequest<IReadOnlyList<Product>>;

public class GetAllProductsQueryHandler(IProductRepository productRepository)
    : IRequestHandler<GetAllProductsQuery, IReadOnlyList<Product>>
{
    public async Task<IReadOnlyList<Product>> Handle(
        GetAllProductsQuery request,
        CancellationToken cancellationToken)
    {
        return await productRepository.GetAllAsync(cancellationToken);
    }
}
