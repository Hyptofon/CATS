using Domain.Entities;

namespace Api.Dtos;

public record CreateUserDto(
    string Email,
    string FirstName,
    string? MiddleName,
    string LastName,
    UserRole Role,
    bool IsActive
);

public record UpdateUserDto(
    string? FirstName,
    string? MiddleName,
    string? LastName,
    UserRole? Role
);

public record UpdateProfileDto(
    string FirstName,
    string? MiddleName,
    string LastName
);

public record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string? MiddleName,
    string LastName,
    UserRole Role,
    bool IsActive
)
{
    public static UserDto FromDomainModel(User user)
    {
        return new UserDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.MiddleName,
            user.LastName,
            user.Role,
            user.IsActive
        );
    }
}
