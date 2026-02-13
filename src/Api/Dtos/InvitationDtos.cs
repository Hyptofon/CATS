using Domain.Entities;

namespace Api.Dtos;

public record CreateInvitationDto(
    string Email,
    UserRole Role
);

public record InvitationDto(
    Guid Id,
    string Email,
    UserRole Role,
    DateTime ExpiresAt,
    bool IsUsed,
    DateTime CreatedAt
)
{
    public static InvitationDto FromDomainModel(UserInvitation invitation)
    {
        return new InvitationDto(
            invitation.Id,
            invitation.Email,
            invitation.Role,
            invitation.ExpiresAt,
            invitation.IsUsed,
            invitation.CreatedAt
        );
    }
}
