using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.ProductTypes.Exceptions;
using Domain.Products;
using LanguageExt;
using MediatR;

namespace Application.ProductTypes.Commands;

public record CreateProductTypeCommand : IRequest<Either<ProductTypeException, ProductType>>
{
    public required string Name { get; init; }
    public int? ShelfLifeDays { get; init; }
    public int? ShelfLifeHours { get; init; }
    public string? Meta { get; init; }
}

public class CreateProductTypeCommandHandler(
    IProductTypeRepository productTypeRepository,
    ICurrentUserService currentUserService,
    IApplicationDbContext dbContext)
    : IRequestHandler<CreateProductTypeCommand, Either<ProductTypeException, ProductType>>
{
    public async Task<Either<ProductTypeException, ProductType>> Handle(
        CreateProductTypeCommand request,
        CancellationToken cancellationToken)
    {
        var existingProductType = await productTypeRepository.GetByNameAsync(
            request.Name, 
            cancellationToken);

        return await existingProductType.MatchAsync(
            pt => new ProductTypeAlreadyExistException(pt.Id),
            () => CreateEntity(request, cancellationToken));
    }

    private async Task<Either<ProductTypeException, ProductType>> CreateEntity(
        CreateProductTypeCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var productType = ProductType.New(
                request.Name,
                request.ShelfLifeDays,
                request.ShelfLifeHours,
                request.Meta,
                currentUserService.UserId);

            productTypeRepository.Add(productType);
            
            await dbContext.SaveChangesAsync(cancellationToken);

            return productType;
        }
        catch (Exception exception)
        {
            return new UnhandledProductTypeException(0, exception);
        }
    }
}