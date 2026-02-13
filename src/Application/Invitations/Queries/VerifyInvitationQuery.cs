using Application.Common.Interfaces;
using Domain.Entities;
using LanguageExt;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Invitations.Queries;

public record VerifyInvitationQuery(Guid Token) : IRequest<Option<UserInvitation>>;

public class VerifyInvitationQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<VerifyInvitationQuery, Option<UserInvitation>>
{
    public async Task<Option<UserInvitation>> Handle(
        VerifyInvitationQuery request,
        CancellationToken cancellationToken)
    {
        var invitation = await dbContext.UserInvitations
            .FirstOrDefaultAsync(i => i.Id == request.Token, cancellationToken);
            
        if (invitation == null)
            return Option<UserInvitation>.None;
            
        if (invitation.IsUsed || invitation.ExpiresAt < DateTime.UtcNow)
            return Option<UserInvitation>.None;

        return invitation;
    }
}
