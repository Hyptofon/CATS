using Application.Common.Interfaces;
using Application.Users.Exceptions;
using Domain.Entities;
using LanguageExt;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Commands;

public record UpdateUserCommand : IRequest<Either<UserException, User>>
{
    public Guid UserId { get; init; }
    public string? FirstName { get; init; }
    public string? MiddleName { get; init; }
    public string? LastName { get; init; }
    public UserRole? Role { get; init; }
}

public class UpdateUserCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<UpdateUserCommand, Either<UserException, User>>
{
    public async Task<Either<UserException, User>> Handle(
        UpdateUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            return new UserNotFoundException(request.UserId);
        }

        if (!string.IsNullOrWhiteSpace(request.FirstName)) user.FirstName = request.FirstName;
        if (request.MiddleName != null) user.MiddleName = request.MiddleName;
        if (!string.IsNullOrWhiteSpace(request.LastName)) user.LastName = request.LastName;
        if (request.Role.HasValue) user.Role = request.Role.Value;

        await dbContext.SaveChangesAsync(cancellationToken);

        return user;
    }
}
