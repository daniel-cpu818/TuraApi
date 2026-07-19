using BookingTura.Application.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;

namespace BookingTura.Infrastructure.Services;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IConfiguration configuration)
    {
    Console.WriteLine("CloudName: " + configuration["Cloudinary:CloudName"]);
    Console.WriteLine("ApiKey: " + configuration["Cloudinary:ApiKey"]);
    Console.WriteLine("ApiSecret: " + configuration["Cloudinary:ApiSecret"]);
        var account = new Account(
            configuration["Cloudinary:CloudName"],
            configuration["Cloudinary:ApiKey"],
            configuration["Cloudinary:ApiSecret"]);

        _cloudinary = new Cloudinary(account);
    }

    public async Task<string> UploadImageAsync(Stream stream, string fileName)
    {
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, stream),
            Folder = "bookingtura"
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.Error != null)
            throw new Exception(result.Error.Message);

        return result.SecureUrl.ToString();
    }

    public async Task DeleteImageAsync(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return;

        var uri = new Uri(imageUrl);

        var segments = uri.AbsolutePath.Split('/');

        var uploadIndex = Array.IndexOf(segments, "upload");

        var publicId = string.Join("/",
            segments.Skip(uploadIndex + 2));

        publicId = Path.ChangeExtension(publicId, null);

        await _cloudinary.DestroyAsync(new DeletionParams(publicId));
    }
}
