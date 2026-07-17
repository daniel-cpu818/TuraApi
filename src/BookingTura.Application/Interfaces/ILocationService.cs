using BookingTura.Application.DTOs;

namespace BookingTura.Application.Interfaces;

public interface ILocationService
{
    Task<List<CreateLocationDto>> GetAllAsync();
    Task<CreateLocationDto?> GetByIdAsync(Guid id);
    Task<CreateLocationDto> CreateAsync(CreateLocationDto dto);
    Task<CreateLocationDto?> FindMatchAsync(CreateLocationDto dto);
}