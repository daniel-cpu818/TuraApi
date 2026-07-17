namespace BookingTura.Application.Interfaces;

public interface IFileUpload
{
    string FileName { get; }

    string? ContentType { get; }

    long Length { get; }

    Stream OpenReadStream();
}
