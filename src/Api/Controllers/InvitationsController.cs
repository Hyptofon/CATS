using Api.Dtos;
using Api.Modules.Errors;
using Application.Invitations.Commands;
using Application.Invitations.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Управління запрошеннями користувачів. Адміністратор створює запрошення на email,
/// після чого користувач може зареєструватися за посиланням-токеном.
/// </summary>
[ApiController]
[Route("invitations")]
public class InvitationsController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Створити запрошення для нового користувача (тільки Admin)
    /// </summary>
    /// <param name="request">Email та роль для запрошеного користувача</param>
    /// <returns>Створене запрошення з токеном і терміном дії</returns>
    /// <response code="200">Запрошення успішно створено</response>
    /// <response code="409">Користувач з таким email вже існує або запрошення вже надіслано</response>
    [Authorize(Roles = "Admin", Policy = "MustBeActive")]
    [HttpPost]
    [ProducesResponseType(typeof(InvitationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(CreateInvitationDto request)
    {
        var command = new CreateInvitationCommand
        {
            Email = request.Email,
            Role = request.Role
        };
        
        var result = await sender.Send(command);
        return result.Match<IActionResult>(
            invitation => Ok(InvitationDto.FromDomainModel(invitation)),
            exception => exception.ToObjectResult());
    }

    /// <summary>
    /// Перевірити запрошення по токену. Використовується при реєстрації нового користувача.
    /// </summary>
    /// <param name="token">Унікальний токен запрошення (GUID)</param>
    /// <returns>Дані запрошення, якщо токен дійсний</returns>
    /// <response code="200">Токен дійсний, запрошення знайдено</response>
    /// <response code="404">Токен невалідний або термін дії запрошення сплив</response>
    [HttpGet("verify/{token}")]
    [ProducesResponseType(typeof(InvitationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Verify(Guid token)
    {
        var result = await sender.Send(new VerifyInvitationQuery(token));
        
        return result.Match<IActionResult>(
            invitation => Ok(InvitationDto.FromDomainModel(invitation)),
            () => NotFound("Invalid or expired invitation."));
    }
}
