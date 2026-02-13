namespace Domain.Entities;

using Common;

public enum UserRole
{
    Operator = 0,   // Звичайний доступ (чекає підтвердження)
    Admin = 1       // Повний доступ
}

public class User : BaseAuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty; // Головний ідентифікатор від Google
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } 
}