using Application.Users.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Api.Modules.Errors;

public static class UserErrorFactory
{
    public static ObjectResult ToObjectResult(this UserException error)
    {
        return new ObjectResult(error.Message)
        {
            StatusCode = error switch
            {
                UserNotFoundException => StatusCodes.Status404NotFound,
                UserAlreadyExistsException => StatusCodes.Status409Conflict,
                InvitationAlreadyExistsException => StatusCodes.Status409Conflict,
                UserNotActiveException => StatusCodes.Status403Forbidden,
                SelfDeactivationException => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            }
        };
    }
}
