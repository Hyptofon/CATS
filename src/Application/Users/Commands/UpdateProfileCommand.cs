using Application.Common.Interfaces;
using Application.Users.Exceptions;
using Domain.Entities;
using LanguageExt;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Commands;

public record UpdateProfileCommand : IRequest<Either<UserException, User>>
{
    public required string FirstName { get; init; }
    public string? MiddleName { get; init; }
    public required string LastName { get; init; }
}

public class UpdateProfileCommandHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService)
    : IRequestHandler<UpdateProfileCommand, Either<UserException, User>>
{
    public async Task<Either<UserException, User>> Handle(
        UpdateProfileCommand request,
        CancellationToken cancellationToken)
    {
        if (currentUserService.UserId == null)
        {
            return new UserNotFoundException(Guid.Empty); 
        }
        
        var userId = currentUserService.UserId.Value;
        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
            
        if (user == null)
        {
            return new UserNotFoundException(userId);
        }

        user.FirstName = request.FirstName;
        user.MiddleName = request.MiddleName;
        user.LastName = request.LastName;
        
        await dbContext.SaveChangesAsync(cancellationToken);
        
        return user;
    }
}
