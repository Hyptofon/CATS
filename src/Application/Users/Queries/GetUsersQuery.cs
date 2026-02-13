using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Queries;

public record GetUsersQuery : IRequest<IReadOnlyList<User>>;

public class GetUsersQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetUsersQuery, IReadOnlyList<User>>
{
    public async Task<IReadOnlyList<User>> Handle(
        GetUsersQuery request,
        CancellationToken cancellationToken)
    {
        return await dbContext.Users
            .AsNoTracking()
            .OrderBy(u => u.Email)
            .ToListAsync(cancellationToken);
    }
}
