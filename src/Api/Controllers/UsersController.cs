using Api.Dtos;
using Api.Modules.Errors;
using Application.Users.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Управління користувачами системи (тільки для адміністраторів).
/// Дозволяє створювати, оновлювати, активувати та деактивувати користувачів.
/// </summary>
[ApiController]
[Route("users")]
[Authorize(Roles = "Admin", Policy = "MustBeActive")]
public class UsersController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Отримати список усіх користувачів системи
    /// </summary>
    /// <returns>Масив усіх користувачів з їх ролями та статусами</returns>
    /// <response code="200">Список користувачів успішно повернуто</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var users = await sender.Send(new Application.Users.Queries.GetUsersQuery());
        return Ok(users.Select(UserDto.FromDomainModel));
    }

    /// <summary>
    /// Створити нового користувача вручну (без запрошення)
    /// </summary>
    /// <param name="request">Дані нового користувача</param>
    /// <returns>Створений користувач</returns>
    /// <response code="200">Користувача успішно створено</response>
    /// <response code="409">Користувач з таким email вже існує</response>
    [HttpPost]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(CreateUserDto request)
    {
        var command = new CreateUserCommand
        {
            Email = request.Email,
            FirstName = request.FirstName,
            MiddleName = request.MiddleName,
            LastName = request.LastName,
            Role = request.Role,
            IsActive = request.IsActive
        };
        
        var result = await sender.Send(command);
        return result.Match<IActionResult>(
            user => Ok(UserDto.FromDomainModel(user)),
            exception => exception.ToObjectResult());
    }

    /// <summary>
    /// Оновити дані користувача (ім'я, роль)
    /// </summary>
    /// <param name="id">ID користувача</param>
    /// <param name="request">Нові дані користувача</param>
    /// <returns>Оновлений користувач</returns>
    /// <response code="200">Користувача успішно оновлено</response>
    /// <response code="404">Користувача з таким ID не знайдено</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, UpdateUserDto request)
    {
        var command = new UpdateUserCommand
        {
            UserId = id,
            FirstName = request.FirstName,
            MiddleName = request.MiddleName,
            LastName = request.LastName,
            Role = request.Role
        };
        
        var result = await sender.Send(command);
        return result.Match<IActionResult>(
            user => Ok(UserDto.FromDomainModel(user)),
            exception => exception.ToObjectResult());
    }

    /// <summary>
    /// Активувати користувача (дозволити доступ до системи)
    /// </summary>
    /// <param name="id">ID користувача</param>
    /// <returns>Пустий OK, якщо успішно</returns>
    /// <response code="200">Користувача успішно активовано</response>
    /// <response code="404">Користувача з таким ID не знайдено</response>
    [HttpPost("{id}/activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(Guid id)
    {
        var result = await sender.Send(new ActivateUserCommand(id));
        return result.Match<IActionResult>(
            _ => Ok(),
            exception => exception.ToObjectResult());
    }

    /// <summary>
    /// Деактивувати користувача (заблокувати доступ до системи)
    /// </summary>
    /// <param name="id">ID користувача</param>
    /// <returns>Пустий OK, якщо успішно</returns>
    /// <response code="200">Користувача успішно деактивовано</response>
    /// <response code="404">Користувача з таким ID не знайдено</response>
    [HttpPost("{id}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var result = await sender.Send(new DeactivateUserCommand(id));
        return result.Match<IActionResult>(
            _ => Ok(),
            exception => exception.ToObjectResult());
    }
}
