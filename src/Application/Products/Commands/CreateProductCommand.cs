using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Products.Exceptions;
using Domain.Products;
using LanguageExt;
using MediatR;

namespace Application.Products.Commands;

public record CreateProductCommand : IRequest<Either<ProductException, Product>>
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required int ProductTypeId { get; init; }
    public int? ShelfLifeDays { get; init; }
    public int? ShelfLifeHours { get; init; }
}

public class CreateProductCommandHandler(
    IProductRepository productRepository,
    IProductTypeRepository productTypeRepository,
    ICurrentUserService currentUserService,
    IApplicationDbContext dbContext)
    : IRequestHandler<CreateProductCommand, Either<ProductException, Product>>
{
    public async Task<Either<ProductException, Product>> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        var productType = await productTypeRepository.GetByIdAsync(
            request.ProductTypeId,
            cancellationToken);

        return await productType.MatchAsync(
            async pt => 
            {
                var shelfLifeDays = request.ShelfLifeDays;
                var shelfLifeHours = request.ShelfLifeHours;

                if (shelfLifeDays == null && shelfLifeHours == null)
                {
                     shelfLifeDays = pt.ShelfLifeDays;
                     shelfLifeHours = pt.ShelfLifeHours;
                }

                return await CreateEntity(
                    request.Name,
                    request.Description,
                    request.ProductTypeId,
                    shelfLifeDays,
                    shelfLifeHours,
                    cancellationToken);
            },
            () => Task.FromResult<Either<ProductException, Product>>(
                new ProductTypeNotFoundForProductException(request.ProductTypeId)));
    }

    private async Task<Either<ProductException, Product>> CreateEntity(
        string name,
        string? description,
        int productTypeId,
        int? shelfLifeDays,
        int? shelfLifeHours,
        CancellationToken cancellationToken)
    {
        try
        {
            var product = Product.New(
                name,
                description,
                productTypeId,
                shelfLifeDays,
                shelfLifeHours,
                currentUserService.UserId);

            productRepository.Add(product);
            await dbContext.SaveChangesAsync(cancellationToken);

            var createdProduct = await productRepository.GetByIdAsync(
                product.Id,
                cancellationToken);

            return createdProduct.Match<Either<ProductException, Product>>(
                some => some,
                () => new ProductNotFoundException(product.Id));
        }
        catch (Exception exception)
        {
            return new UnhandledProductException(0, exception);
        }
    }
}
