using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Products.Exceptions;
using Domain.Products;
using LanguageExt;
using MediatR;

namespace Application.Products.Commands;

public record UpdateProductCommand : IRequest<Either<ProductException, Product>>
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required int ProductTypeId { get; init; }
}

public class UpdateProductCommandHandler(
    IProductRepository productRepository,
    IProductTypeRepository productTypeRepository,
    ICurrentUserService currentUserService,
    IApplicationDbContext dbContext)
    : IRequestHandler<UpdateProductCommand, Either<ProductException, Product>>
{
    public async Task<Either<ProductException, Product>> Handle(
        UpdateProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.Id, cancellationToken);

        return await product.MatchAsync(
            async p =>
            {
                var productType = await productTypeRepository.GetByIdAsync(
                    request.ProductTypeId,
                    cancellationToken);

                return await productType.MatchAsync(
                    pt => UpdateEntity(p, request, cancellationToken),
                    () => Task.FromResult<Either<ProductException, Product>>(
                        new UnhandledProductException(request.Id, 
                            new ProductTypeNotFoundForProductException(request.ProductTypeId))));
            },
            () => Task.FromResult<Either<ProductException, Product>>(
                new ProductNotFoundException(request.Id)));
    }

    private async Task<Either<ProductException, Product>> UpdateEntity(
        Product product,
        UpdateProductCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            product.UpdateDetails(
                request.Name,
                request.Description,
                request.ProductTypeId,
                currentUserService.UserId);

            productRepository.Update(product);
            await dbContext.SaveChangesAsync(cancellationToken);

            return product;
        }
        catch (Exception exception)
        {
            return new UnhandledProductException(request.Id, exception);
        }
    }
}
