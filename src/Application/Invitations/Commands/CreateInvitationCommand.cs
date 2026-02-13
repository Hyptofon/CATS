using Application.Common.Interfaces;
using Application.Users.Exceptions;
using Domain.Entities;
using LanguageExt;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Application.Invitations.Commands;

public record CreateInvitationCommand : IRequest<Either<UserException, UserInvitation>>
{
    public required string Email { get; init; }
    public required UserRole Role { get; init; }
}

public class CreateInvitationCommandHandler(
    IApplicationDbContext dbContext,
    IEmailSender emailSender,
    IConfiguration configuration)
    : IRequestHandler<CreateInvitationCommand, Either<UserException, UserInvitation>>
{
    public async Task<Either<UserException, UserInvitation>> Handle(
        CreateInvitationCommand request,
        CancellationToken cancellationToken)
    {
        var userExists = await dbContext.Users
            .AnyAsync(u => u.Email == request.Email, cancellationToken);
            
        if (userExists)
        {
            return new UserAlreadyExistsException(request.Email);
        }
        
        var existingInvite = await dbContext.UserInvitations
            .FirstOrDefaultAsync(i => 
                i.Email == request.Email && !i.IsUsed && i.ExpiresAt > DateTime.UtcNow, cancellationToken);

        if (existingInvite != null)
        {
            return new InvitationAlreadyExistsException(request.Email);
        }
        
        var invitation = new UserInvitation
        {
            Id = Guid.NewGuid(), 
            Email = request.Email,
            Role = request.Role,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsUsed = false,
        };

        dbContext.UserInvitations.Add(invitation);
        await dbContext.SaveChangesAsync(cancellationToken);

        // Send Email (Fire and forget to not block)
        var baseUrl = configuration["App:FrontendUrl"] ?? "http://localhost:5208/dev-auth.html";
        var inviteLink = $"{baseUrl}?token={invitation.Id}"; 
        var subject = "Welcome to CATS!";
        var body = $@"
            <h3>You have been invited to join CATS!</h3>
            <p>Your role: <b>{invitation.Role}</b></p>
            <p>Click the link below to accept the invitation:</p>
            <a href='{inviteLink}'>Accept Invitation</a>
            <br/>
            <small>This link expires on {invitation.ExpiresAt}</small>";
            
        _ = emailSender.SendEmailAsync(request.Email, subject, body);

        return invitation;
    }
}
