using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BookingTura.Application.Interfaces;

namespace BookingTura.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ICurrentUserService _currentUser;

    public AuthController(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }
    // 🔐 Obtener usuario autenticado

[Authorize]
[HttpGet("me")]
public IActionResult Me()
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var email = User.FindFirst("https://bookingtura-api/email")?.Value;
    return Ok(new
    {
        userId,
        email
    });
}

    // 🔥 Sincronizar usuario con DB
    [Authorize]
    [HttpPost("sync")]
    public async Task<IActionResult> Sync(
        [FromServices] IUserService userService)
    {
        var user = await userService.GetOrCreateUserAsync();
        var allClaims = User.Claims.Select(c => new { c.Type, c.Value });
        var email = User.FindFirst("https://bookingtura-api/email")?.Value
         ?? User.FindFirst(ClaimTypes.Email)?.Value;
        return Ok(user);
    }

[Authorize]
[HttpGet("debug")]
public IActionResult Debug()
{
    return Ok(User.Claims.Select(c => new { c.Type, c.Value }));
}
}