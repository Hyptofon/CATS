using Application.Common.Interfaces;
using Application.Users.Exceptions;
using Domain.Entities;
using LanguageExt;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Application.Invitations.Commands;

public record ResendInvitationCommand : IRequest<Either<UserException, UserInvitation>>
{
    public required Guid InvitationId { get; init; }
}

public class ResendInvitationCommandHandler(
    IApplicationDbContext dbContext,
    IEmailSender emailSender,
    IConfiguration configuration)
    : IRequestHandler<ResendInvitationCommand, Either<UserException, UserInvitation>>
{
    public async Task<Either<UserException, UserInvitation>> Handle(
        ResendInvitationCommand request,
        CancellationToken cancellationToken)
    {
        var invitation = await dbContext.UserInvitations
            .FirstOrDefaultAsync(i => i.Id == request.InvitationId, cancellationToken);

        if (invitation == null)
        {
            return new InvitationNotFoundException(request.InvitationId);
        }

        if (invitation.IsUsed)
        {
            return new UserAlreadyExistsException(invitation.Email);
        }

        invitation.ExpiresAt = DateTime.UtcNow.AddDays(1);

        await dbContext.SaveChangesAsync(cancellationToken);

        // Send Email (Fire and forget to not block)
        var baseUrl = configuration["App:FrontendUrl"] ?? "http://localhost:5208/dev-auth.html";
        var inviteLink = $"{baseUrl}?token={invitation.Id}"; 
        var subject = "Welcome to CATS! Your invitation has been resent";
        var body = $@"
            <h3>You have been invited to join CATS!</h3>
            <p>Your role: <b>{invitation.Role}</b></p>
            <p>Click the link below to accept the invitation:</p>
            <a href='{inviteLink}'>Accept Invitation</a>
            <br/>
            <small>This link expires on {invitation.ExpiresAt}</small>";
            
        _ = emailSender.SendEmailAsync(invitation.Email, subject, body);

        return invitation;
    }
}
