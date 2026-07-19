namespace BookingTura.Application.Interfaces;

public interface ICloudinaryService
{
    Task<string> UploadImageAsync(Stream stream, string fileName);

    Task DeleteImageAsync(string imageUrl);
}
