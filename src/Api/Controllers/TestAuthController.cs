using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Common.Interfaces;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestAuthController : ControllerBase
{
    private readonly ICurrentUserService _currentUser;

    public TestAuthController(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    // 1. Доступно ВСІМ авторизованим (показує твою роль)
    [HttpGet("who-am-i")]
    [Authorize] 
    public IActionResult WhoAmI()
    {
        // Витягуємо ролі з токена
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        return Ok(new 
        { 
            Message = "Ти в системі!",
            MyInternalId = _currentUser.UserId,
            MyEmail = _currentUser.Email,
            MyRoles = roles, 
            IsAuthenticated = _currentUser.IsAuthenticated
        });
    }

    // 2. Доступно ТІЛЬКИ Адмінам
    [HttpGet("admin-secret")]
    [Authorize(Roles = "Admin")]
    public IActionResult GetAdminSecret()
    {
        return Ok("😎 Вітаю, Великий Адмін! Тобі можна все.");
    }

    // 3. Доступно І Адмінам, І Операторам (базовий доступ)
    [HttpGet("operator-zone")]
    [Authorize(Roles = "Admin,Operator")]
    public IActionResult GetOperatorZone()
    {
        return Ok("📦 Це зона операторів. Тут ми рухаємо тару.");
    }
}