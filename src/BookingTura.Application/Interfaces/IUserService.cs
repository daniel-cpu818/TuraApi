using BookingTura.Domain.Entities;

namespace BookingTura.Application.Interfaces;

public interface IUserService
{
    Task<User> GetOrCreateUserAsync();
    Task<User> CompleteProfileAsync(CompleteProfileDto dto);
}