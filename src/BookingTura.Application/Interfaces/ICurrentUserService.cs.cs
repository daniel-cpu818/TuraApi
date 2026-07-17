using BookingTura.Domain.Entities;

namespace BookingTura.Application.Interfaces;

public interface ICurrentUserService
{
    string? Auth0Id { get; }
    string? Email { get; }
}