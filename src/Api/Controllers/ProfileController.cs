using Api.Dtos;
using Api.Modules.Errors;
using Application.Users.Commands;
using Application.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Профіль поточного авторизованого користувача.
/// Дозволяє переглядати та оновлювати свої особисті дані (ім'я, прізвище).
/// </summary>
[ApiController]
[Route("profile")]
[Authorize(Policy = "MustBeActive")]
public class ProfileController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Отримати профіль поточного авторизованого користувача
    /// </summary>
    /// <returns>Дані профілю (email, ім'я, роль, статус активності)</returns>
    /// <response code="200">Профіль успішно повернуто</response>
    /// <response code="401">Користувач не авторизований</response>
    [HttpGet]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Get()
    {
        var result = await sender.Send(new GetProfileQuery());
        return result.Match<IActionResult>(
            user => Ok(UserDto.FromDomainModel(user)),
            exception => exception.ToObjectResult());
    }

    /// <summary>
    /// Оновити профіль поточного користувача (ім'я, по батькові, прізвище)
    /// </summary>
    /// <param name="request">Нові дані профілю</param>
    /// <returns>Оновлений профіль</returns>
    /// <response code="200">Профіль успішно оновлено</response>
    /// <response code="401">Користувач не авторизований</response>
    [HttpPut]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(UpdateProfileDto request)
    {
        var command = new UpdateProfileCommand
        {
            FirstName = request.FirstName,
            MiddleName = request.MiddleName,
            LastName = request.LastName
        };
        
        var result = await sender.Send(command);
        return result.Match<IActionResult>(
            user => Ok(UserDto.FromDomainModel(user)),
            exception => exception.ToObjectResult());
    }
}
