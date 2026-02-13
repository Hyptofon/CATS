using Application.Products.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Api.Modules.Errors;

public static class ProductErrorFactory
{
    public static ObjectResult ToObjectResult(this ProductException error)
    {
        return new ObjectResult(error.Message)
        {
            StatusCode = error switch
            {
                ProductNotFoundException => StatusCodes.Status404NotFound,
                ProductTypeNotFoundForProductException => StatusCodes.Status404NotFound,
                ProductInUseException => StatusCodes.Status400BadRequest,
                UnhandledProductException => StatusCodes.Status500InternalServerError,
                _ => throw new NotImplementedException("Product error handler not implemented")
            }
        };
    }
}
