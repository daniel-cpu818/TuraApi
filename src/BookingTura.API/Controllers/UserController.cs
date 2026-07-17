using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BookingTura.Application.Interfaces;

namespace BookingTura.API.Controllers;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    // 🔥 Obtener usuario actual
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser(
        [FromServices] IUserService userService)
    {
        var user = await userService.GetOrCreateUserAsync();
        return Ok(user);
    }

    [Authorize]
    [HttpPost("complete-profile")]
    public async Task<IActionResult> CompleteProfile(
        [FromBody] CompleteProfileDto dto,
        [FromServices] IUserService userService)
    {
        var user = await userService.CompleteProfileAsync(dto);
        return Ok(user);
    }
}

