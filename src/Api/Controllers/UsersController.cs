using Api.Dtos;
using Api.Modules.Errors;
using Application.Users.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("users")]
[Authorize(Roles = "Admin")]
public class UsersController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await sender.Send(new Application.Users.Queries.GetUsersQuery());
        return Ok(users.Select(UserDto.FromDomainModel));
    }

    [HttpPost]
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

    [HttpPut("{id}")]
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

    [HttpPost("{id}/activate")]
    public async Task<IActionResult> Activate(Guid id)
    {
        var result = await sender.Send(new ActivateUserCommand(id));
        return result.Match<IActionResult>(
            _ => Ok(),
            exception => exception.ToObjectResult());
    }

    [HttpPost("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var result = await sender.Send(new DeactivateUserCommand(id));
        return result.Match<IActionResult>(
            _ => Ok(),
            exception => exception.ToObjectResult());
    }
}
