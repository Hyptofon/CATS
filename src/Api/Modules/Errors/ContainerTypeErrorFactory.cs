using Application.ContainerTypes.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Api.Modules.Errors;

public static class ContainerTypeErrorFactory
{
    public static ObjectResult ToObjectResult(this ContainerTypeException error)
    {
        return new ObjectResult(error.Message)
        {
            StatusCode = error switch
            {
                ContainerTypeAlreadyExistException => StatusCodes.Status409Conflict,
                ContainerTypeNotFoundException => StatusCodes.Status404NotFound,
                ContainerTypeCannotBeDeletedException => StatusCodes.Status409Conflict,
                UnhandledContainerTypeException => StatusCodes.Status500InternalServerError,
                _ => throw new NotImplementedException(
                    "Container type error handler not implemented")
            }
        };
    }
}