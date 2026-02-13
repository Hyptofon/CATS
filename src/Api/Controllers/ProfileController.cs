using Api.Dtos;
using Api.Modules.Errors;
using Application.Users.Commands;
using Application.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("profile")]
[Authorize]
public class ProfileController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await sender.Send(new GetProfileQuery());
        return result.Match<IActionResult>(
            user => Ok(UserDto.FromDomainModel(user)),
            exception => exception.ToObjectResult());
    }

    [HttpPut]
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
