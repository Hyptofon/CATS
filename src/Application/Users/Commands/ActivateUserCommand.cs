using Application.Common.Interfaces;
using Application.Users.Exceptions;
using Domain.Entities;
using LanguageExt;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Commands;

public record ActivateUserCommand(Guid UserId) : IRequest<Either<UserException, LanguageExt.Unit>>;

public class ActivateUserCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<ActivateUserCommand, Either<UserException, LanguageExt.Unit>>
{
    public async Task<Either<UserException, LanguageExt.Unit>> Handle(
        ActivateUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            return new UserNotFoundException(request.UserId);
        }

        user.IsActive = true;
        await dbContext.SaveChangesAsync(cancellationToken);

        return LanguageExt.Unit.Default;
    }
}
