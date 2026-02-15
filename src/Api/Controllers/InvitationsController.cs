using Api.Dtos;
using Api.Modules.Errors;
using Application.Invitations.Commands;
using Application.Invitations.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("invitations")]
public class InvitationsController(ISender sender) : ControllerBase
{
    [Authorize(Roles = "Admin", Policy = "MustBeActive")]
    [HttpPost]
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

    [HttpGet("verify/{token}")]
    public async Task<IActionResult> Verify(Guid token)
    {
        var result = await sender.Send(new VerifyInvitationQuery(token));
        
        return result.Match<IActionResult>(
            invitation => Ok(InvitationDto.FromDomainModel(invitation)),
            () => NotFound("Invalid or expired invitation."));
    }
}
