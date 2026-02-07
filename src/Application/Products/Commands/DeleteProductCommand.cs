using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Products.Exceptions;
using Domain.Products;
using LanguageExt;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Products.Commands;

public record DeleteProductCommand : IRequest<Either<ProductException, Product>>
{
    public required int Id { get; init; }
}

public class DeleteProductCommandHandler(
    IProductRepository productRepository,
    IApplicationDbContext dbContext)
    : IRequestHandler<DeleteProductCommand, Either<ProductException, Product>>
{
    public async Task<Either<ProductException, Product>> Handle(
        DeleteProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.Id, cancellationToken);

        return await product.MatchAsync(
            async p =>
            {
                var isInUse = await IsProductInUseAsync(p.Id, cancellationToken);
                if (isInUse)
                {
                    return (Either<ProductException, Product>)new ProductInUseException(p.Id);
                }

                productRepository.Delete(p);
                await dbContext.SaveChangesAsync(cancellationToken);
                
                return (Either<ProductException, Product>)p;
            },
            () => Task.FromResult<Either<ProductException, Product>>(
                new ProductNotFoundException(request.Id)));
    }

    private async Task<bool> IsProductInUseAsync(int productId, CancellationToken cancellationToken)
    {
        return await dbContext.ContainerFills
            .AnyAsync(cf => cf.ProductId == productId, cancellationToken);
    }
}
