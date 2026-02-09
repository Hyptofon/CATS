using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.ProductTypes.Exceptions;
using Domain.Products;
using LanguageExt;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.ProductTypes.Commands;

public record DeleteProductTypeCommand(int ProductTypeId) 
    : IRequest<Either<ProductTypeException, ProductType>>;

public class DeleteProductTypeCommandHandler(
    IProductTypeRepository productTypeRepository,
    IApplicationDbContext dbContext)
    : IRequestHandler<DeleteProductTypeCommand, Either<ProductTypeException, ProductType>>
{
    public async Task<Either<ProductTypeException, ProductType>> Handle(
        DeleteProductTypeCommand request,
        CancellationToken cancellationToken)
    {
        var productTypeId = request.ProductTypeId;
        
        var hasProducts = await dbContext.Products
            .AnyAsync(p => p.ProductTypeId == productTypeId && !p.IsDeleted, cancellationToken);
        
        if (hasProducts)
        {
            return new ProductTypeInUseException(productTypeId);
        }
        
        var existingProductType = await productTypeRepository.GetByIdAsync(
            productTypeId, 
            cancellationToken);

        return await existingProductType.MatchAsync(
            productType => DeleteEntity(productType, cancellationToken),
            () => Task.FromResult<Either<ProductTypeException, ProductType>>(
                new ProductTypeNotFoundException(productTypeId)));
    }

    private async Task<Either<ProductTypeException, ProductType>> DeleteEntity(
        ProductType productType,
        CancellationToken cancellationToken)
    {
        try
        {
            productTypeRepository.Delete(productType);
            
            await dbContext.SaveChangesAsync(cancellationToken);
            
            return productType;
        }
        catch (Exception exception)
        {
            return new UnhandledProductTypeException(productType.Id, exception);
        }
    }
}