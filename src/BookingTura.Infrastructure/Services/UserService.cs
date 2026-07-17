// using BookingTura.Application.Interfaces;
using BookingTura.Domain.Entities;
using BookingTura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using BookingTura.Application.Interfaces;

namespace BookingTura.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly BookingTuraDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UserService(
        BookingTuraDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<User> GetOrCreateUserAsync()
{
    var auth0Id = _currentUser.Auth0Id;
    var email = _currentUser.Email;

    // 🚨 VALIDACIÓN CLAVE
    if (string.IsNullOrEmpty(auth0Id))
        throw new Exception("Usuario no autenticado (Auth0Id es null)");

    var user = await _context.Users
        .FirstOrDefaultAsync(u => u.Auth0Id == auth0Id);

    if (user != null)
        return user;

    user = new User
    {
        Auth0Id = auth0Id,
        Email = email ?? "no-email@local",
        Name = email ?? "Usuario",
        Role = Domain.Enums.UserRole.Customer
    };

    _context.Users.Add(user);
    await _context.SaveChangesAsync();

    return user;
}
public async Task<User> CompleteProfileAsync(CompleteProfileDto dto)
{
    var auth0Id = _currentUser.Auth0Id;

    var user = await _context.Users
        .FirstOrDefaultAsync(u => u.Auth0Id == auth0Id);

    if (user == null)
        throw new Exception("Usuario no encontrado");

    // 🔥 actualizar datos
    user.Name = dto.Name;
    user.Phone = dto.Phone;
    user.UpdatedAt = DateTime.UtcNow;

    await _context.SaveChangesAsync();

    return user;
}
}