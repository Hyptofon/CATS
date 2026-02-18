using Domain.Entities;

namespace Api.Dtos;

/// <summary>
/// Дані для створення запрошення користувача
/// </summary>
public record CreateInvitationDto
{
    /// <summary>Email адреса запрошуваного користувача (обов'язкове)</summary>
    public required string Email { get; init; }
    /// <summary>Роль, яка буде призначена користувачу: Admin або Operator</summary>
    public required UserRole Role { get; init; }
}

/// <summary>
/// Повна інформація про запрошення (відповідь API)
/// </summary>
public record InvitationDto
{
    /// <summary>Унікальний ідентифікатор (токен) запрошення</summary>
    public required Guid Id { get; init; }
    /// <summary>Email адреса запрошеного користувача</summary>
    public required string Email { get; init; }
    /// <summary>Роль, яка буде призначена: Admin або Operator</summary>
    public required UserRole Role { get; init; }
    /// <summary>Дата та час, до якого запрошення дійсне</summary>
    public required DateTime ExpiresAt { get; init; }
    /// <summary>Чи було запрошення вже використане (true — користувач зареєструвався)</summary>
    public required bool IsUsed { get; init; }
    /// <summary>Дата та час створення запрошення</summary>
    public required DateTime CreatedAt { get; init; }

    public static InvitationDto FromDomainModel(UserInvitation invitation)
    {
        return new InvitationDto
        {
            Id = invitation.Id,
            Email = invitation.Email,
            Role = invitation.Role,
            ExpiresAt = invitation.ExpiresAt,
            IsUsed = invitation.IsUsed,
            CreatedAt = invitation.CreatedAt
        };
    }
}
