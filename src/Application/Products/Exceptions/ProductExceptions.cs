namespace Application.Products.Exceptions;

public abstract class ProductException(
    int productId, 
    string message, 
    Exception? innerException = null)
    : Exception(message, innerException)
{
    public int ProductId { get; } = productId;
}

public sealed class ProductNotFoundException(int productId) 
    : ProductException(productId, $"Product not found under id {productId}");

public sealed class ProductTypeNotFoundForProductException(int productTypeId)
    : Exception($"Product type with id {productTypeId} not found for product operation");

public sealed class ProductInUseException(int productId) 
    : ProductException(productId, $"Product with id {productId} is in use by active containers and cannot be deleted");

public sealed class UnhandledProductException(int productId, Exception? innerException = null)
    : ProductException(productId, "Unexpected error occurred", innerException);
