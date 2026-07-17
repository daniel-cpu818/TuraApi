using BookingTura.Application.DTOs.Properties;
using BookingTura.Application.DTOs.Publications;
using BookingTura.Application.Interfaces;
using BookingTura.Domain.Entities;
using BookingTura.Domain.Enums;
using BookingTura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingTura.Infrastructure.Services;

public class PublicationService : IPublicationService
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
    private readonly IGeocodingService _geocodingService;

    public PublicationService(
        BookingTuraDbContext context,
        ICurrentUserService currentUser,
        IGeocodingService geocodingService)
    {
        _context = context;
        _currentUser = currentUser;
        _geocodingService = geocodingService;
    }

    public async Task<PublicationResponseDto> DeleteAsync(Guid id)
    {
        var publication = await _context.Publications
            .Include(p => p.Property)
                .ThenInclude(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (publication == null)
            throw new Exception("Publicacion no encontrada.");

        var user = await GetCurrentUserAsync();
        if (publication.Property.OwnerId != user.Id)
            throw new Exception("No tiene permisos para eliminar esta publicacion.");

        _context.Publications.Remove(publication);
        _context.Properties.Remove(publication.Property);
        await _context.SaveChangesAsync();

        var imageUrls = publication.Property.Images
            .Select(image => image.Url!)
            .Where(url => !string.IsNullOrWhiteSpace(url))
            .ToList();

        DeleteFiles(imageUrls);

        return MapToDto(
            publication,
            publication.Property,
            user,
            publication.Property.Location,
            publication.Property.PropertyType);
    }

    public async Task<PublicationResponseDto> UpdateAsync(
        Guid id,
        UpdatePublicationDto dto)
    {
        var publication = await _context.Publications
        .Include(p => p.Property)
            .ThenInclude(p => p.Images)
        .Include(p => p.Property)
            .ThenInclude(p => p.Location)
        .Include(p => p.Property)
            .ThenInclude(p => p.PropertyType)
        .FirstOrDefaultAsync(p => p.Id == id);

        if (publication == null)
            throw new Exception("Publicacion no encontrada.");

        var user = await GetCurrentUserAsync();
        if (publication.Property.OwnerId != user.Id)
            throw new Exception("No tiene permisos para actualizar esta publicacion.");

        ValidatePublication(dto.Type, dto.StartDate, dto.EndDate);

        publication.Property.Title = dto.Title;
        publication.Property.Description = dto.Description;
        publication.Property.Price = dto.Price;
        publication.Property.PropertyTypeId = dto.PropertyTypeId;

        publication.Property.Location.Hood = dto.Hood;
        publication.Property.Location.Commune = dto.Commune;
        publication.Property.Location.Piso = dto.Piso;
        publication.Property.Location.Address = dto.Address;

        publication.Property.Location.Latitude = dto.Latitude;
        publication.Property.Location.Longitude = dto.Longitude;

        publication.Property.UpdatedAt = DateTime.UtcNow;
        _context.Publications.Update(publication);
        await _context.SaveChangesAsync();

        return MapToDto(
            publication,
            publication.Property,
            user,
            publication.Property.Location,
            publication.Property.PropertyType);
    }

    public async Task<PublicationResponseDto> CreateAsync(
        CreatePublicationDto dto,
        IEnumerable<IFileUpload>? images = null)
    {
        ValidatePublication(dto.Type, dto.StartDate, dto.EndDate);

        var user = await GetCurrentUserAsync();
        var uploads = NormalizeUploads(images);
        ValidateUploadsSafe(uploads);

        var propertyType = await GetPropertyTypeAsync(dto.PropertyTypeId);
        var location = await ResolveLocationAsync(
            dto.Hood,
            dto.Piso,
            dto.Commune,
            dto.Address,
            dto.Latitude,
            dto.Longitude);

        var property = new Property
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            Price = dto.Price,
            Hood = NormalizeText(dto.Hood),
            Address = dto.Address,
            Commune = dto.Commune,
            Latitude = ToNullableDecimal(location.Latitude) ?? ToNullableDecimal(dto.Latitude),
            Longitude = ToNullableDecimal(location.Longitude) ?? ToNullableDecimal(dto.Longitude),
            OwnerId = user.Id,
            Owner = user,
            PropertyTypeId = propertyType.Id,
            PropertyType = propertyType,
            LocationId = location.Id,
            Location = location,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var publication = new Publication
        {
            Id = Guid.NewGuid(),
            PropertyId = property.Id,
            Property = property,
            Type = dto.Type,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        await using var transaction = await _context.Database.BeginTransactionAsync();
        var savedImageUrls = new List<string>();

        try
        {
            _context.Properties.Add(property);
            _context.Publications.Add(publication);
            await _context.SaveChangesAsync();

            if (uploads.Count > 0)
            {
                var propertyImages = await CreatePropertyImagesAsync(property, uploads);
                savedImageUrls.AddRange(propertyImages
                    .Select(image => image.Url!)
                    .Where(url => !string.IsNullOrWhiteSpace(url)));

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

        return MapToDto(publication, property, user, location, propertyType);
    }


    public async Task<IEnumerable<PublicationResponseDto>> GetMyPublicationsAsync()
{
    var user = await GetCurrentUserAsync();

    var publications = await _context.Publications
        .Include(p => p.Property)
            .ThenInclude(p => p.Owner)
        .Include(p => p.Property)
            .ThenInclude(p => p.Location)
        .Include(p => p.Property)
            .ThenInclude(p => p.PropertyType)
        .Include(p => p.Property)
            .ThenInclude(p => p.Images)
        .Where(p => p.Property.OwnerId == user.Id)
        .OrderByDescending(p => p.CreatedAt)
        .ToListAsync();

    return publications.Select(publication =>
    {
        var property = publication.Property;

        return new PublicationResponseDto
        {
            Id = publication.Id,
            PropertyId = property.Id,
            Type = publication.Type,
            StartDate = publication.StartDate,
            EndDate = publication.EndDate,
            IsActive = publication.IsActive,

            Property = new PropertyResponseDto
            {
                Id = property.Id,
                Title = property.Title,
                Description = property.Description,
                Price = property.Price,

                PropertyTypeId = property.PropertyTypeId,
                PropertyTypeName = property.PropertyType?.Name,

                OwnerName = property.Owner?.Name,
                OwnerEmail = property.Owner?.Email,
                OwnerPhone = property.Owner?.Phone,

                Address = property.Location?.Address,
                Commune = property.Location?.Commune,
                Hood = property.Location?.Hood,
                Piso = property.Location?.Piso,

                Latitude = property.Location?.Latitude,
                Longitude = property.Location?.Longitude,

                Images = property.Images
                    .Select(i => new PropertyImageResponseDto
                    {
                        Id = i.Id,
                        Url = i.Url,
                        IsMain = i.IsMain
                    })
                    .ToList()
            }
        };
    });
}

    public async Task<IEnumerable<PublicationResponseDto>> GetAllAsync()
    {
        var publications = await _context.Publications
            .Include(publication => publication.Property)
                .ThenInclude(property => property.Owner)
            .Include(publication => publication.Property)
                .ThenInclude(property => property.Location)
            .Include(publication => publication.Property)
                .ThenInclude(property => property.PropertyType)
            .Include(publication => publication.Property)
                .ThenInclude(property => property.Images)
            .OrderByDescending(publication => publication.CreatedAt)
            .ToListAsync();

        return publications.Select(publication =>
            MapToDto(
                publication,
                publication.Property,
                publication.Property?.Owner,
                publication.Property?.Location,
                publication.Property?.PropertyType));
    }

    public async Task<PublicationResponseDto?> GetByIdAsync(Guid id)
    {
        var publication = await _context.Publications
            .Include(current => current.Property)
                .ThenInclude(property => property.Owner)
            .Include(current => current.Property)
                .ThenInclude(property => property.Location)
            .Include(current => current.Property)
                .ThenInclude(property => property.PropertyType)
            .Include(current => current.Property)
                .ThenInclude(property => property.Images)
            .FirstOrDefaultAsync(current => current.Id == id);

        if (publication == null)
            return null;

        return MapToDto(
            publication,
            publication.Property,
            publication.Property?.Owner,
            publication.Property?.Location,
            publication.Property?.PropertyType);
    }

    private async Task<User> GetCurrentUserAsync()
    {
        var auth0Id = _currentUser.Auth0Id;

        if (string.IsNullOrWhiteSpace(auth0Id))
            throw new Exception("Usuario no autenticado");

        var user = await _context.Users
            .FirstOrDefaultAsync(current => current.Auth0Id == auth0Id);

        if (user == null)
            throw new Exception("Usuario no existe en DB");

        return user;
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
            throw new Exception("La direccion es obligatoria");

        var normalizedHood = NormalizeText(hood);
        var normalizedPiso = NormalizeText(piso);
        var normalizedCommune = NormalizeText(commune);

        var existing = await _context.Locations.FirstOrDefaultAsync(location =>
            location.Hood == normalizedHood &&
            location.Piso == normalizedPiso &&
            location.Commune == normalizedCommune &&
            location.Address == address);

        if (existing != null)
        {
            if (!HasCoordinates(existing))
            {
                var coordinates = await ResolveCoordinatesAsync(normalizedHood, normalizedPiso, normalizedCommune, address, latitude, longitude);
                ApplyCoordinates(existing, coordinates);
            }

            return existing;
        }

        var resolvedCoordinates = await ResolveCoordinatesAsync(normalizedHood, normalizedPiso, normalizedCommune, address, latitude, longitude);

        var location = new Location
        {
            Id = Guid.NewGuid(),
            Hood = normalizedHood,
            Piso = normalizedPiso,
            Commune = normalizedCommune,
            Address = address,
            Latitude = resolvedCoordinates?.Latitude,
            Longitude = resolvedCoordinates?.Longitude
        };

        _context.Locations.Add(location);
        return location;
    }

     private async Task<(double Latitude, double Longitude)?> ResolveCoordinatesAsync(
        string? hood,
        string? piso,
        string? commune,
        string address,
        double? latitude,
        double? longitude)
    {
        try
        {
            var geocoded = await _geocodingService.GeocodeAsync(address, commune, hood, piso);
            if (geocoded != null)
                return (geocoded.Latitude, geocoded.Longitude);
        }
        catch
        {
        }

        if (latitude.HasValue && longitude.HasValue)
            return (latitude.Value, longitude.Value);

        return null;
    }

    private static bool HasCoordinates(Location location)
    {
        return location.Latitude.HasValue && location.Longitude.HasValue;
    }

    private static void ApplyCoordinates(
        Location location,
        (double Latitude, double Longitude)? coordinates)
    {
        if (coordinates is null)
            return;

        location.Latitude = coordinates.Value.Latitude;
        location.Longitude = coordinates.Value.Longitude;
    }


    private async Task<PropertyType> GetPropertyTypeAsync(Guid propertyTypeId)
    {
        if (propertyTypeId == Guid.Empty)
            throw new Exception("No se recibio un propertyTypeId valido.");

        var propertyType = await _context.PropertyTypes
            .FirstOrDefaultAsync(current => current.Id == propertyTypeId);

        if (propertyType == null)
            throw new Exception($"Tipo de propiedad invalido: {propertyTypeId}");

        return propertyType;
    }

    private static void ValidatePublication(
        PublicationType type,
        DateTime startDate,
        DateTime endDate)
    {
        if (!Enum.IsDefined(type))
            throw new ArgumentException("El tipo de publicacion no es valido.");

        if (startDate == default || endDate == default)
            throw new ArgumentException("Las fechas de publicacion son obligatorias.");

        if (endDate <= startDate)
            throw new ArgumentException("La fecha final debe ser mayor que la fecha inicial.");
    }

    private static List<IFileUpload> NormalizeUploads(IEnumerable<IFileUpload>? images)
    {
        return images?
            .Where(image => image.Length > 0)
            .ToList()
            ?? [];
    }

    private static void ValidateUploads(IReadOnlyCollection<IFileUpload> uploads)
    {
        if (uploads.Count > MaxImagesPerProperty)
            throw new Exception($"Solo se permiten {MaxImagesPerProperty} imagenes por propiedad.");

        foreach (var upload in uploads)
        {
            if (upload.Length <= 0)
                throw new Exception("No se permiten imagenes vacias.");

            if (upload.Length > MaxImageSizeBytes)
                throw new Exception("Cada imagen debe pesar maximo 5 MB.");

            var extension = Path.GetExtension(upload.FileName);
            if (string.IsNullOrWhiteSpace(extension) || !AllowedImageExtensions.Contains(extension))
                throw new Exception("Solo se permiten imagenes JPG, PNG o WEBP.");

            if (!string.IsNullOrWhiteSpace(upload.ContentType)
                && !upload.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("El archivo enviado no es una imagen valida.");
            }
        }
    }

    private static void ValidateUploadsSafe(IReadOnlyCollection<IFileUpload> uploads)
    {
        if (uploads.Count > MaxImagesPerProperty)
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
                IsMain = index == 0,
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
                    File.Delete(fullPath);
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }

    private static PublicationResponseDto MapToDto(
        Publication publication,
        Property? property,
        User? owner,
        Location? location,
        PropertyType? type)
    {
        return new PublicationResponseDto
        {
            Id = publication.Id,
            PropertyId = publication.PropertyId,
            Type = publication.Type,
            StartDate = publication.StartDate,
            EndDate = publication.EndDate,
            IsActive = publication.IsActive,
            Property = property == null
                ? new PropertyResponseDto()
                : MapPropertyToDto(property, owner, location, type)
        };
    }

    private static PropertyResponseDto MapPropertyToDto(
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
            OwnerPhone = owner?.Phone,
            Address = location?.Address,
            Latitude = location?.Latitude ?? ToNullableDouble(property.Latitude),
            Longitude = location?.Longitude ?? ToNullableDouble(property.Longitude),
            Commune = location?.Commune,
            Hood = location?.Hood,
            Piso = location?.Piso,
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

    private static string NormalizeText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }

    private static decimal? ToNullableDecimal(double? value)
    {
        return value.HasValue ? (decimal?)Convert.ToDecimal(value.Value) : null;
    }

    private static decimal? ToNullableDecimal(decimal? value)
    {
        return value;
    }

    private static double? ToNullableDouble(decimal? value)
    {
        return value.HasValue ? Convert.ToDouble(value.Value) : null;
    }
}
