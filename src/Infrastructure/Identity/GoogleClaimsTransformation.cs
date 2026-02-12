using System.Security.Claims;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Identity;

public class GoogleClaimsTransformation : IClaimsTransformation
{
    private readonly IServiceProvider _serviceProvider;

    public GoogleClaimsTransformation(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        // 1. Перевірка: чи користувач взагалі авторизований
        var identity = principal.Identity as ClaimsIdentity;
        if (identity == null || !identity.IsAuthenticated) return principal;

        // 2. Отримуємо Email з токена Google
        var email = identity.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email)) return principal;

        // 3. Створюємо Scope для доступу до бази даних (бо ClaimsTransformation - Singleton/Transient)
        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // 4. Шукаємо користувача в БД
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);

            // 5. ЛОГІКА РЕЄСТРАЦІЇ
            if (user == null)
            {
                // Перевіряємо, чи є хоч хтось у базі
                var isFirstUser = !await context.Users.AnyAsync();

                user = new User
                {
                    Email = email,
                    FirstName = identity.FindFirst(ClaimTypes.GivenName)?.Value ?? "Unknown",
                    LastName = identity.FindFirst(ClaimTypes.Surname)?.Value ?? "Unknown",
                    
                    // Якщо перший - Адмін, інакше - Оператор
                    Role = isFirstUser ? UserRole.Admin : UserRole.Operator,
                    
                    // Якщо перший - Активний, інакше - Неактивний (чекає підтвердження)
                    IsActive = isFirstUser,
                    CreatedAt = DateTime.UtcNow
                };

                context.Users.Add(user);
                await context.SaveChangesAsync();
            }

            // 6. Якщо користувач не активний - не даємо йому роль (доступ буде заборонено)
            if (!user.IsActive)
            {
                return principal;
            }

            // 7. Додаємо роль і ID у "паспорт" запиту (Claims)
            if (!principal.HasClaim(c => c.Type == ClaimTypes.Role))
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, user.Role.ToString()));
                // Додаємо наш внутрішній ID, щоб використовувати його в аудиті
                identity.AddClaim(new Claim("UserId", user.Id.ToString())); 
            }
        }

        return principal;
    }
}