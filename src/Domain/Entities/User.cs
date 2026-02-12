namespace Domain.Entities;

public enum UserRole
{
    Operator = 0,   // Звичайний доступ (чекає підтвердження)
    Admin = 1       // Повний доступ
}

public class User 
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty; // Головний ідентифікатор від Google
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    
    public UserRole Role { get; set; }
    
    // Якщо true - користувач може працювати. Якщо false - чекає апрува адміна.
    public bool IsActive { get; set; } 
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}