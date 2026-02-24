using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Invitations.Queries;

public record GetInvitationsQuery : IRequest<List<UserInvitation>>;

public class GetInvitationsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetInvitationsQuery, List<UserInvitation>>
{
    public async Task<List<UserInvitation>> Handle(
        GetInvitationsQuery request,
        CancellationToken cancellationToken)
    {
        return await dbContext.UserInvitations
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
