using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.ProductTypes.Exceptions;
using Domain.Products;
using LanguageExt;
using MediatR;

namespace Application.ProductTypes.Commands;

public record UpdateProductTypeCommand : IRequest<Either<ProductTypeException, ProductType>>
{
    public required int ProductTypeId { get; init; }
    public required string Name { get; init; }
    public int? ShelfLifeDays { get; init; }
    public string? Meta { get; init; }
}

public class UpdateProductTypeCommandHandler(
    IProductTypeRepository productTypeRepository,
    ICurrentUserService currentUserService,
    IApplicationDbContext dbContext)
    : IRequestHandler<UpdateProductTypeCommand, Either<ProductTypeException, ProductType>>
{
    public async Task<Either<ProductTypeException, ProductType>> Handle(
        UpdateProductTypeCommand request,
        CancellationToken cancellationToken)
    {
        var productTypeId = request.ProductTypeId;
        var existingProductType = await productTypeRepository.GetByIdAsync(
            productTypeId, 
            cancellationToken);

        return await existingProductType.MatchAsync(
            productType => UpdateEntity(productType, request, cancellationToken),
            () => Task.FromResult<Either<ProductTypeException, ProductType>>(
                new ProductTypeNotFoundException(productTypeId)));
    }

    private async Task<Either<ProductTypeException, ProductType>> UpdateEntity(
        ProductType productType,
        UpdateProductTypeCommand request,
        CancellationToken cancellationToken)
    {
        var existingProductTypeWithSameName = await productTypeRepository
            .GetByNameAsync(request.Name, cancellationToken);

        if (existingProductTypeWithSameName.IsSome 
            && existingProductTypeWithSameName.Map(pt => pt.Id != productType.Id).IfNone(false))
        {
            return new ProductTypeAlreadyExistException(productType.Id);
        }
        
        try
        {
            productType.UpdateDetails(
                request.Name,
                request.ShelfLifeDays,
                request.Meta, 
                currentUserService.UserId);
            
            productTypeRepository.Update(productType);
            
            await dbContext.SaveChangesAsync(cancellationToken);
            
            return productType;
        }
        catch (Exception exception)
        {
            return new UnhandledProductTypeException(productType.Id, exception);
        }
    }
}