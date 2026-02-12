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

    // Цей метод доступний ВСІМ авторизованим (і Адмінам, і Операторам)
    [HttpGet("who-am-i")]
    [Authorize] 
    public IActionResult WhoAmI()
    {
        return Ok(new 
        { 
            Message = "Ти в системі!",
            MyInternalId = _currentUser.UserId,
            MyEmail = _currentUser.Email,
            IsAuthenticated = _currentUser.IsAuthenticated
        });
    }

    // Цей метод доступний ТІЛЬКИ Адмінам
    [HttpGet("admin-secret")]
    [Authorize(Roles = "Admin")]
    public IActionResult GetAdminSecret()
    {
        return Ok("Вітаю, Великий Адмін! Це секретна інформація.");
    }
}