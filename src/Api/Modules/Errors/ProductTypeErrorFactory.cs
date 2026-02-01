using Application.ProductTypes.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Api.Modules.Errors;

public static class ProductTypeErrorFactory
{
    public static ObjectResult ToObjectResult(this ProductTypeException error)
    {
        return new ObjectResult(error.Message)
        {
            StatusCode = error switch
            {
                ProductTypeAlreadyExistException => StatusCodes.Status409Conflict,
                ProductTypeNotFoundException => StatusCodes.Status404NotFound,
                UnhandledProductTypeException => StatusCodes.Status500InternalServerError,
                _ => throw new NotImplementedException(
                    "Product type error handler not implemented")
            }
        };
    }
}