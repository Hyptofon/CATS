using Application.Common.Interfaces;
using Application.Users.Exceptions;
using Domain.Entities;
using LanguageExt;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Queries;

public record GetProfileQuery : IRequest<Either<UserException, User>>;

public class GetProfileQueryHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetProfileQuery, Either<UserException, User>>
{
    public async Task<Either<UserException, User>> Handle(
        GetProfileQuery request,
        CancellationToken cancellationToken)
    {
        if (currentUserService.UserId == null)
            return new UserNotFoundException(Guid.Empty);

        var userId = currentUserService.UserId.Value;
        var user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
            
        if (user == null)
            return new UserNotFoundException(userId);

        return user;
    }
}
