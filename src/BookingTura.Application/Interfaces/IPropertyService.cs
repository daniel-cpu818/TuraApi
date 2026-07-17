using BookingTura.Application.DTOs.Properties;

namespace BookingTura.Application.Interfaces;

public interface IPropertyService
{
    Task<PropertyResponseDto> CreateAsync(
        CreatePropertyDto dto,
        IEnumerable<IFileUpload>? images = null);

    Task<IEnumerable<PropertyResponseDto>> GetAllAsync();

    Task<PropertyResponseDto?> GetByIdAsync(Guid id);

    Task<PropertyResponseDto?> UpdateAsync(
        Guid id,
        UpdatePropertyDto dto,
        IEnumerable<IFileUpload>? images = null,
        bool replaceImages = false);

    Task<bool> DeleteAsync(Guid id);
}
