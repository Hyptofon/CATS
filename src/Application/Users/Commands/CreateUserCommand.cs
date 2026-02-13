using Application.Common.Interfaces;
using Application.Users.Exceptions;
using Domain.Entities;
using LanguageExt;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Commands;

public record CreateUserCommand : IRequest<Either<UserException, User>>
{
    public required string Email { get; init; }
    public required string FirstName { get; init; }
    public string? MiddleName { get; init; }
    public required string LastName { get; init; }
    public UserRole Role { get; init; }
    public bool IsActive { get; init; }
}

public class CreateUserCommandHandler(
    IApplicationDbContext dbContext,
    IEmailSender emailSender,
    Microsoft.Extensions.Configuration.IConfiguration configuration)
    : IRequestHandler<CreateUserCommand, Either<UserException, User>>
{
    public async Task<Either<UserException, User>> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken)
    {
        var existingUser = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (existingUser != null)
        {
            return new UserAlreadyExistsException(request.Email);
        }

        var user = new User
        {
            Email = request.Email,
            FirstName = request.FirstName,
            MiddleName = request.MiddleName,
            LastName = request.LastName,
            Role = request.Role,
            IsActive = request.IsActive
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        // Send Welcome Email if created as Inactive (or even Active, to notify them)
        try 
        {
            var baseUrl = configuration["App:FrontendUrl"] ?? "http://localhost:5208/dev-auth.html";
            var loginLink = baseUrl; 
            var subject = "Welcome to CATS!";
            var body = $@"
                <h3>Welcome to CATS, {user.FirstName}!</h3>
                <p>An account has been created for you with role: <b>{user.Role}</b>.</p>
                <p>Please log in with your Google account to access the system:</p>
                <a href='{loginLink}'>Log in to CATS</a>";
                
            _ = emailSender.SendEmailAsync(request.Email, subject, body);
        }
        catch
        {
            // Suppress email errors to not fail the user creation
        }

        return user;
    }
}
