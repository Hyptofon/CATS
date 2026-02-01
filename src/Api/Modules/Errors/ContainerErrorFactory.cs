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
                UnhandledContainerException => StatusCodes.Status500InternalServerError,
                _ => throw new NotImplementedException(
                    "Container error handler not implemented")
            }
        };
    }
}