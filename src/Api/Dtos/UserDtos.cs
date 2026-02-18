using Domain.Entities;

namespace Api.Dtos;

/// <summary>
/// Дані для створення нового користувача вручну (адміністратором)
/// </summary>
public record CreateUserDto
{
    /// <summary>Email адреса користувача (обов'язкове, унікальне)</summary>
    public required string Email { get; init; }
    /// <summary>Ім'я користувача</summary>
    public required string FirstName { get; init; }
    /// <summary>По батькові (необов'язкове)</summary>
    public string? MiddleName { get; init; }
    /// <summary>Прізвище користувача</summary>
    public required string LastName { get; init; }
    /// <summary>Роль користувача: Admin або Operator</summary>
    public required UserRole Role { get; init; }
    /// <summary>Чи активний користувач (true — має доступ до системи)</summary>
    public required bool IsActive { get; init; }
}

/// <summary>
/// Дані для оновлення користувача адміністратором. Усі поля необов'язкові — передавайте тільки ті, що змінюються.
/// </summary>
public record UpdateUserDto
{
    /// <summary>Нове ім'я</summary>
    public string? FirstName { get; init; }
    /// <summary>Нове по батькові</summary>
    public string? MiddleName { get; init; }
    /// <summary>Нове прізвище</summary>
    public string? LastName { get; init; }
    /// <summary>Нова роль: Admin або Operator</summary>
    public UserRole? Role { get; init; }
}

/// <summary>
/// Дані для оновлення власного профілю
/// </summary>
public record UpdateProfileDto
{
    /// <summary>Нове ім'я (обов'язкове)</summary>
    public required string FirstName { get; init; }
    /// <summary>Нове по батькові</summary>
    public string? MiddleName { get; init; }
    /// <summary>Нове прізвище (обов'язкове)</summary>
    public required string LastName { get; init; }
}

/// <summary>
/// Повна інформація про користувача (відповідь API)
/// </summary>
public record UserDto
{
    /// <summary>Унікальний ідентифікатор користувача (GUID)</summary>
    public required Guid Id { get; init; }
    /// <summary>Email адреса</summary>
    public required string Email { get; init; }
    /// <summary>Ім'я</summary>
    public required string FirstName { get; init; }
    /// <summary>По батькові</summary>
    public string? MiddleName { get; init; }
    /// <summary>Прізвище</summary>
    public required string LastName { get; init; }
    /// <summary>Роль: Admin або Operator</summary>
    public required UserRole Role { get; init; }
    /// <summary>Чи активний (true — має доступ до системи, false — заблокований)</summary>
    public required bool IsActive { get; init; }

    public static UserDto FromDomainModel(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            MiddleName = user.MiddleName,
            LastName = user.LastName,
            Role = user.Role,
            IsActive = user.IsActive
        };
    }
}
