namespace Application.ProductTypes.Exceptions;

public abstract class ProductTypeException(
    int productTypeId, 
    string message, 
    Exception? innerException = null)
    : Exception(message, innerException)
{
    public int ProductTypeId { get; } = productTypeId;
}

public class ProductTypeAlreadyExistException(int productTypeId) 
    : ProductTypeException(productTypeId, $"Product type already exists under id {productTypeId}");

public class ProductTypeNotFoundException(int productTypeId) 
    : ProductTypeException(productTypeId, $"Product type not found under id {productTypeId}");

public class UnhandledProductTypeException(
    int productTypeId, 
    Exception? innerException = null)
    : ProductTypeException(productTypeId, "Unexpected error occurred", innerException);

public class ProductTypeInUseException(int productTypeId)
    : ProductTypeException(productTypeId, $"Product type with id {productTypeId} cannot be deleted because it is used by existing products");