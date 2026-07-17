using System.Security.Claims;
using BookingTura.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace BookingTura.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? Auth0Id =>
        _httpContextAccessor.HttpContext?.User?
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public string? Email =>
        _httpContextAccessor.HttpContext?.User?
            .FindFirst("https://bookingtura-api/email")?.Value;

    public Guid? UserId => null; // opcional por ahora
}