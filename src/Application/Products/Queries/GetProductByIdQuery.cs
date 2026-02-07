using Application.Common.Interfaces.Repositories;
using Domain.Products;
using LanguageExt;
using MediatR;

namespace Application.Products.Queries;

public record GetProductByIdQuery : IRequest<Option<Product>>
{
    public required int Id { get; init; }
}

public class GetProductByIdQueryHandler(IProductRepository productRepository)
    : IRequestHandler<GetProductByIdQuery, Option<Product>>
{
    public async Task<Option<Product>> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await productRepository.GetByIdAsync(request.Id, cancellationToken);
    }
}
