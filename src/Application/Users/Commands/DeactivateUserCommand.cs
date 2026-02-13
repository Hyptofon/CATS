using Application.Common.Interfaces;
using Application.Users.Exceptions;
using Domain.Entities;
using LanguageExt;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Commands;

public record DeactivateUserCommand(Guid UserId) : IRequest<Either<UserException, LanguageExt.Unit>>;

public class DeactivateUserCommandHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService)
    : IRequestHandler<DeactivateUserCommand, Either<UserException, LanguageExt.Unit>>
{
    public async Task<Either<UserException, LanguageExt.Unit>> Handle(
        DeactivateUserCommand request,
        CancellationToken cancellationToken)
    {
        if (request.UserId == currentUserService.UserId)
        {
            return new SelfDeactivationException();
        }

        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            return new UserNotFoundException(request.UserId);
        }

        user.IsActive = false;
        await dbContext.SaveChangesAsync(cancellationToken);

        return LanguageExt.Unit.Default;
    }
}
