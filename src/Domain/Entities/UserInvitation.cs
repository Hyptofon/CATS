using Domain.Common;

namespace Domain.Entities;

public class UserInvitation : BaseAuditableEntity
{
    public Guid Id { get; set; } 
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
}
