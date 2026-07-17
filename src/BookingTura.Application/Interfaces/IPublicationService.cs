using BookingTura.Application.DTOs.Publications;

namespace BookingTura.Application.Interfaces;

public interface IPublicationService
{
    Task<PublicationResponseDto> CreateAsync(
        CreatePublicationDto dto,
        IEnumerable<IFileUpload>? images = null);

    Task<PublicationResponseDto> UpdateAsync(
        Guid id,
        UpdatePublicationDto dto);

    Task<PublicationResponseDto> DeleteAsync(Guid id);

    Task<IEnumerable<PublicationResponseDto>> GetAllAsync();

    Task<IEnumerable<PublicationResponseDto>> GetMyPublicationsAsync();

    Task<PublicationResponseDto?> GetByIdAsync(Guid id);
}
