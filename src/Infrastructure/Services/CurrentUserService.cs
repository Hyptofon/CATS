using System.Security.Claims;
using Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // 1. Реалізація UserId (парсимо string у Guid)
    public Guid? UserId
    {
        get
        {
            var idClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("UserId")?.Value;
            
            if (string.IsNullOrEmpty(idClaim))
            {
                return null;
            }

            return Guid.TryParse(idClaim, out var userId) ? userId : null;
        }
    }

    // 2. Реалізація Email (все по старому)
    public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;

    // 3. Реалізація IsAuthenticated (додали це, бо інтерфейс вимагає)
    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}