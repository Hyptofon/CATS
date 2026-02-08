using Application.Containers.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Api.Modules.Errors;

public static class ContainerErrorFactory
{
    public static ObjectResult ToObjectResult(this ContainerException error)
    {
        return new ObjectResult(error.Message)
        {
            StatusCode = error switch
            {
                ContainerAlreadyExistException => StatusCodes.Status409Conflict,
                ContainerNotFoundException => StatusCodes.Status404NotFound,
                ContainerTypeNotFoundForContainerException => StatusCodes.Status404NotFound,
                ContainerFillNotFoundException => StatusCodes.Status404NotFound,
                ContainerOverfillException => StatusCodes.Status400BadRequest,
                ContainerUnitMismatchException => StatusCodes.Status400BadRequest,
                ContainerNotEmptyException => StatusCodes.Status400BadRequest,
                ContainerNotFullException => StatusCodes.Status400BadRequest,
                UnhandledContainerException => StatusCodes.Status500InternalServerError,
                _ => throw new NotImplementedException(
                    "Container error handler not implemented")
            }
        };
    }
}