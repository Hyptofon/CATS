using Domain.Products;

namespace Application.ProductTypes.Exceptions;

public abstract class ProductTypeException(
    ProductTypeId productTypeId, 
    string message, 
    Exception? innerException = null)
    : Exception(message, innerException)
{
    public ProductTypeId ProductTypeId { get; } = productTypeId;
}

public class ProductTypeAlreadyExistException(ProductTypeId productTypeId) 
    : ProductTypeException(productTypeId, $"Product type already exists under id {productTypeId}");

public class ProductTypeNotFoundException(ProductTypeId productTypeId) 
    : ProductTypeException(productTypeId, $"Product type not found under id {productTypeId}");

public class UnhandledProductTypeException(
    ProductTypeId productTypeId, 
    Exception? innerException = null)
    : ProductTypeException(productTypeId, "Unexpected error occurred", innerException);