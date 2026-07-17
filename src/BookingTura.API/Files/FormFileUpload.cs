using BookingTura.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace BookingTura.API.Files;

public sealed class FormFileUpload(IFormFile file) : IFileUpload
{
    public string FileName => file.FileName;

    public string? ContentType => file.ContentType;

    public long Length => file.Length;

    public Stream OpenReadStream() => file.OpenReadStream();
}
