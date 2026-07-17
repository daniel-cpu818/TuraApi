using BookingTura.Application.DTOs.Properties;
using BookingTura.Application.Interfaces;
using BookingTura.Domain.Entities;
using BookingTura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingTura.Infrastructure.Services;

public class PropertyService : IPropertyService
{
    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".jfif",
        ".png",
        ".webp"
    };

    private const int MaxImagesPerProperty = 10;
    private const long MaxImageSizeBytes = 5 * 1024 * 1024;
    private readonly BookingTuraDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public PropertyService(
        BookingTuraDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PropertyResponseDto> CreateAsync(
        CreatePropertyDto dto,
        IEnumerable<IFileUpload>? images = null)
    {
        var user = await GetCurrentUserAsync();
        var uploads = NormalizeUploads(images);

        ValidateUploadsSafe(uploads, 0, replaceImages: false);

        var propertyType = await GetPropertyTypeAsync(dto.PropertyTypeId);
        var location = await ResolveLocationAsync(dto);

        var property = new Property
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            Price = dto.Price,
            Hood = string.IsNullOrWhiteSpace(location.Hood)
                ? (string.IsNullOrWhiteSpace(dto.Hood) ? string.Empty : dto.Hood.Trim())
                : location.Hood,
            Address = location.Address ?? dto.Address,
            Commune = location.Commune ?? dto.Commune,
            Latitude = ToNullableDecimal(location.Latitude) ?? ToNullableDecimal(dto.Latitude),
            Longitude = ToNullableDecimal(location.Longitude) ?? ToNullableDecimal(dto.Longitude),
            PropertyTypeId = propertyType.Id,
            OwnerId = user.Id,
            Location = location,
            CreatedAt = DateTime.UtcNow
        };

        await using var transaction = await _context.Database.BeginTransactionAsync();
        var savedImageUrls = new List<string>();

        try
        {
            _context.Properties.Add(property);
            await _context.SaveChangesAsync();

            if (uploads.Count > 0)
            {
                var propertyImages = await CreatePropertyImagesAsync(property, uploads);
                savedImageUrls.AddRange(propertyImages.Select(image => image.Url!).Where(url => !string.IsNullOrWhiteSpace(url)));

                _context.PropertyImages.AddRange(propertyImages);
                property.Images = propertyImages;
                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            DeleteFiles(savedImageUrls);
            throw;
        }

        return MapToDto(property, user, location, propertyType);
    }

    public async Task<IEnumerable<PropertyResponseDto>> GetAllAsync()
    {
        var properties = await _context.Properties
            .Include(p => p.Owner)
            .Include(p => p.Location)
            .Include(p => p.PropertyType)
            .Include(p => p.Images)
            .ToListAsync();

        return properties.Select(p => MapToDto(p, p.Owner, p.Location, p.PropertyType));
    }

    public async Task<PropertyResponseDto?> GetByIdAsync(Guid id)
    {
        var property = await _context.Properties
            .Include(p => p.Owner)
            .Include(p => p.Location)
            .Include(p => p.PropertyType)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (property == null)
            return null;

        return MapToDto(property, property.Owner, property.Location, property.PropertyType);
    }

    public async Task<PropertyResponseDto?> UpdateAsync(
        Guid id,
        UpdatePropertyDto dto,
        IEnumerable<IFileUpload>? images = null,
        bool replaceImages = false)
    {
        var user = await GetCurrentUserAsync();
        var uploads = NormalizeUploads(images);

        var property = await _context.Properties
            .Include(p => p.Owner)
            .Include(p => p.Location)
            .Include(p => p.PropertyType)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (property == null)
            return null;

        if (property.OwnerId != user.Id)
            throw new Exception("No tienes permiso");

        ValidateUploadsSafe(uploads, property.Images.Count, replaceImages);

        if (!string.IsNullOrWhiteSpace(dto.Title))
            property.Title = dto.Title;

        if (!string.IsNullOrWhiteSpace(dto.Description))
            property.Description = dto.Description;

        if (dto.Price.HasValue)
            property.Price = dto.Price.Value;

        if (dto.PropertyTypeId.HasValue && dto.PropertyTypeId.Value != property.PropertyTypeId)
        {
            var propertyType = await GetPropertyTypeAsync(dto.PropertyTypeId.Value);
            property.PropertyTypeId = propertyType.Id;
            property.PropertyType = propertyType;
        }

        if (ShouldUpdateLocation(dto))
        {
            property.Location = await ResolveLocationAsync(
                dto.Hood ?? property.Location.Hood,
                dto.Piso ?? property.Location.Piso,
                dto.Commune ?? property.Location.Commune,
                dto.Address ?? property.Location.Address,
            property.Location.Latitude,
            property.Location.Longitude
            );

            property.Latitude = ToNullableDecimal(property.Location.Latitude) ?? property.Latitude;
            property.Longitude = ToNullableDecimal(property.Location.Longitude) ?? property.Longitude;
        }

        var removedImageUrls = replaceImages
            ? property.Images.Select(image => image.Url).Where(url => !string.IsNullOrWhiteSpace(url)).Cast<string>().ToList()
            : [];
        var savedImageUrls = new List<string>();

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            if (replaceImages && property.Images.Count > 0)
            {
                _context.PropertyImages.RemoveRange(property.Images);
                property.Images.Clear();
            }

            if (uploads.Count > 0)
            {
                var newImages = await CreatePropertyImagesAsync(property, uploads);
                savedImageUrls.AddRange(newImages.Select(image => image.Url!).Where(url => !string.IsNullOrWhiteSpace(url)));
                _context.PropertyImages.AddRange(newImages);

                foreach (var image in newImages)
                {
                    property.Images.Add(image);
                }
            }

            property.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            DeleteFiles(savedImageUrls);
            throw;
        }

        DeleteFiles(removedImageUrls);

        return MapToDto(property, property.Owner, property.Location, property.PropertyType);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var user = await GetCurrentUserAsync();

        var property = await _context.Properties
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (property == null)
            return false;

        if (property.OwnerId != user.Id)
            throw new Exception("No tienes permiso");

        var imageUrls = property.Images
            .Select(image => image.Url)
            .Where(url => !string.IsNullOrWhiteSpace(url))
            .Cast<string>()
            .ToList();

        _context.Properties.Remove(property);
        await _context.SaveChangesAsync();
        DeleteFiles(imageUrls);

        return true;
    }

    private async Task<User> GetCurrentUserAsync()
    {
        var auth0Id = _currentUser.Auth0Id;

        if (string.IsNullOrEmpty(auth0Id))
            throw new Exception("Usuario no autenticado");

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Auth0Id == auth0Id);

        if (user == null)
            throw new Exception("Usuario no existe en DB");

        return user;
    }

    private async Task<Location> ResolveLocationAsync(CreatePropertyDto dto)
    {
        return await ResolveLocationAsync(
            dto.Hood,
            dto.Piso,
            dto.Commune,
            dto.Address,
            dto.Latitude,
            dto.Longitude);
    }

    private async Task<Location> ResolveLocationAsync(
        string? hood,
        string? piso,
        string? commune,
        string? address,
        double? latitude,
        double? longitude)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new Exception("La dirección es obligatoria");

        var existing = await _context.Locations.FirstOrDefaultAsync(l =>
            l.Hood == hood &&
            l.Piso == piso &&
            l.Commune == commune &&
            l.Address == address);

        if (existing != null)
            return existing;

        var location = new Location
        {
            Id = Guid.NewGuid(),
            Address = address,
            Hood = hood,
            Piso = piso,
            Commune = commune,
            Latitude = latitude,
            Longitude = longitude
        };

        _context.Locations.Add(location);
        return location;
    }

    private async Task<PropertyType> GetPropertyTypeAsync(Guid propertyTypeId)
    {
        if (propertyTypeId == Guid.Empty)
            throw new Exception("No se recibió un propertyTypeId válido.");

        var propertyType = await _context.PropertyTypes
            .FirstOrDefaultAsync(pt => pt.Id == propertyTypeId);

        if (propertyType == null)
            throw new Exception($"Tipo de propiedad inválido: {propertyTypeId}");

        return propertyType;
    }

    private static bool ShouldUpdateLocation(UpdatePropertyDto dto)
    {
        return dto.Hood is not null
            || dto.Piso is not null
            || dto.Commune is not null
            || dto.Address is not null;
    }

    private static decimal? ToNullableDecimal(double? value)
    {
        return value.HasValue ? (decimal?)Convert.ToDecimal(value.Value) : null;
    }

    private static double? ToNullableDouble(decimal? value)
    {
        return value.HasValue ? Convert.ToDouble(value.Value) : null;
    }

    private static List<IFileUpload> NormalizeUploads(IEnumerable<IFileUpload>? images)
    {
        return images?
            .Where(image => image.Length > 0)
            .ToList()
            ?? [];
    }

    private static void ValidateUploads(
        IReadOnlyCollection<IFileUpload> uploads,
        int existingImagesCount,
        bool replaceImages)
    {
        var finalCount = replaceImages ? uploads.Count : existingImagesCount + uploads.Count;

        if (finalCount > MaxImagesPerProperty)
            throw new Exception($"Solo se permiten {MaxImagesPerProperty} imágenes por propiedad.");

        foreach (var upload in uploads)
        {
            if (upload.Length <= 0)
                throw new Exception("No se permiten imágenes vacías.");

            if (upload.Length > MaxImageSizeBytes)
                throw new Exception("Cada imagen debe pesar máximo 5 MB.");

            var extension = Path.GetExtension(upload.FileName);
            if (string.IsNullOrWhiteSpace(extension) || !AllowedImageExtensions.Contains(extension))
                throw new Exception("Solo se permiten imágenes JPG, PNG o WEBP.");

            if (!string.IsNullOrWhiteSpace(upload.ContentType)
                && !upload.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("El archivo enviado no es una imagen válida.");
            }
        }
    }

    private static void ValidateUploadsSafe(
        IReadOnlyCollection<IFileUpload> uploads,
        int existingImagesCount,
        bool replaceImages)
    {
        var finalCount = replaceImages ? uploads.Count : existingImagesCount + uploads.Count;

        if (finalCount > MaxImagesPerProperty)
            throw new ArgumentException($"Solo se permiten {MaxImagesPerProperty} imagenes por propiedad.");

        foreach (var upload in uploads)
        {
            if (upload.Length <= 0)
                throw new ArgumentException("No se permiten imagenes vacias.");

            if (upload.Length > MaxImageSizeBytes)
                throw new ArgumentException("Cada imagen debe pesar maximo 5 MB.");

            var extension = Path.GetExtension(upload.FileName);
            if (string.IsNullOrWhiteSpace(extension) || !AllowedImageExtensions.Contains(extension))
            {
                var fileName = string.IsNullOrWhiteSpace(upload.FileName) ? "(sin nombre)" : upload.FileName;
                var receivedExtension = string.IsNullOrWhiteSpace(extension) ? "(sin extension)" : extension;
                throw new ArgumentException(
                    $"Formato de imagen no soportado para '{fileName}'. Extension recibida: '{receivedExtension}'. Formatos permitidos: .jpg, .jpeg, .jfif, .png, .webp.");
            }

            if (!string.IsNullOrWhiteSpace(upload.ContentType)
                && !upload.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    $"El archivo '{upload.FileName}' no tiene un content-type de imagen valido. Se recibio: '{upload.ContentType}'.");
            }
        }
    }

    private static async Task<List<PropertyImage>> CreatePropertyImagesAsync(
        Property property,
        IReadOnlyCollection<IFileUpload> uploads)
    {
        if (uploads.Count == 0)
            return [];

        var uploadDirectory = GetUploadDirectory();
        Directory.CreateDirectory(uploadDirectory);

        var hasMainImage = property.Images.Any(image => image.IsMain);
        var createdImages = new List<PropertyImage>(uploads.Count);
        var index = 0;

        foreach (var upload in uploads)
        {
            var extension = Path.GetExtension(upload.FileName);
            var fileName = $"{property.Id:N}-{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadDirectory, fileName);

            await using (var sourceStream = upload.OpenReadStream())
            await using (var targetStream = new FileStream(filePath, FileMode.Create))
            {
                await sourceStream.CopyToAsync(targetStream);
            }

            createdImages.Add(new PropertyImage
            {
                Id = Guid.NewGuid(),
                PropertyId = property.Id,
                Property = property,
                Url = $"/uploads/properties/{fileName}",
                IsMain = !hasMainImage && index == 0,
                CreatedAt = DateTime.UtcNow
            });

            index++;
        }

        return createdImages;
    }

    private static string GetUploadDirectory()
    {
        return Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "uploads",
            "properties");
    }

    private static void DeleteFiles(IEnumerable<string> imageUrls)
    {
        foreach (var imageUrl in imageUrls)
        {
            var relativePath = imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath);

            try
            {
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }

    private static PropertyResponseDto MapToDto(
        Property property,
        User? owner,
        Location? location,
        PropertyType? type)
    {
        return new PropertyResponseDto
        {
            Id = property.Id,
            Title = property.Title,
            Description = property.Description,
            Price = property.Price,
            PropertyTypeId = property.PropertyTypeId,
            PropertyTypeName = type?.Name,
            OwnerName = owner?.Name,
            OwnerEmail = owner?.Email,
            Address = location?.Address,
            Commune = location?.Commune,
            Hood = location?.Hood,
            Piso = location?.Piso,
            Latitude = location?.Latitude ?? ToNullableDouble(property.Latitude),
            Longitude = location?.Longitude ?? ToNullableDouble(property.Longitude),
            Images = property.Images
                .OrderByDescending(image => image.IsMain)
                .ThenBy(image => image.CreatedAt)
                .Select(image => new PropertyImageResponseDto
                {
                    Id = image.Id,
                    Url = image.Url,
                    IsMain = image.IsMain
                })
                .ToList()
        };
    }
}
