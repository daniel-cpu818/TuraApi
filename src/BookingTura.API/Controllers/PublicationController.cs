using BookingTura.API.Files;
using BookingTura.API.Models.Publications;
using BookingTura.Application.DTOs.Publications;
using BookingTura.Application.Interfaces;
using BookingTura.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookingTura.API.Controllers;

[ApiController]
[Route("api/publications")]
public class PublicationController : ControllerBase
{
    private readonly IPublicationService _service;

    public PublicationController(IPublicationService service)
    {
        _service = service;
    }

    [Authorize]
    [HttpPost]
    [Consumes("application/json")]
    public async Task<IActionResult> Create([FromBody] CreatePublicationDto dto)
    {
        var validation = ValidateRequest(dto.PropertyTypeId, dto.StartDate, dto.EndDate, dto.Type);
        if (validation is not null)
            return validation;

        try
        {
            var publication = await _service.CreateAsync(dto);
            return Ok(publication);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (Exception exception)
        {
            return HandlePublicationException(exception);
        }
    }

    [Authorize]
    [HttpPost("with-images")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateWithImages([FromForm] CreatePublicationFormRequest request)
    {
        var validation = ValidateRequest(request.PropertyTypeId, request.StartDate, request.EndDate, request.Type);
        if (validation is not null)
            return validation;

        try
        {
            var publication = await _service.CreateAsync(
                MapCreateDto(request),
                MapFiles(request.Images));

            return Ok(publication);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (Exception exception)
        {
            return HandlePublicationException(exception);
        }
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMyPublications()
    {
        try
        {
            var publications = await _service.GetMyPublicationsAsync();
            return Ok(publications);
        }
        catch (Exception exception)
        {
            return HandlePublicationException(exception);
        }
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var publication = await _service.GetByIdAsync(id);
        return publication == null ? NotFound() : Ok(publication);
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    [Consumes("application/json")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePublicationDto dto)
    {
        var validation = ValidatePublicationDates(dto.StartDate, dto.EndDate, dto.Type);
        if (validation is not null)
            return validation;

        try
        {
            var publication = await _service.UpdateAsync(id, dto);
            return Ok(publication);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (Exception exception)
        {
            return HandlePublicationException(exception);
        }
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _service.DeleteAsync(id);
            return Ok();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (Exception exception)
        {
            return HandlePublicationException(exception);
        }
    }

    private IActionResult? ValidateRequest(
        Guid propertyTypeId,
        DateTime startDate,
        DateTime endDate,
        PublicationType type)
    {
        if (propertyTypeId == Guid.Empty)
        {
            return BadRequest(new
            {
                message = "El campo propertyTypeId es obligatorio y debe ser un GUID valido."
            });
        }

        return ValidatePublicationDates(startDate, endDate, type);
    }

    private IActionResult? ValidatePublicationDates(
        DateTime startDate,
        DateTime endDate,
        PublicationType type)
    {
        if (!Enum.IsDefined(type))
        {
            return BadRequest(new
            {
                message = "El tipo de publicacion enviado no es valido."
            });
        }

        if (startDate == default || endDate == default)
        {
            return BadRequest(new
            {
                message = "startDate y endDate son obligatorios."
            });
        }

        if (endDate <= startDate)
        {
            return BadRequest(new
            {
                message = "endDate debe ser mayor que startDate."
            });
        }

        return null;
    }

    private IActionResult HandlePublicationException(Exception exception)
    {
        var message = exception.Message;

        if (message.Contains("autenticado", StringComparison.OrdinalIgnoreCase))
        {
            return Unauthorized(new { message });
        }

        if (message.Contains("permiso", StringComparison.OrdinalIgnoreCase))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message });
        }

        if (message.Contains("no encontrada", StringComparison.OrdinalIgnoreCase)
            || message.Contains("no existe", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(new { message });
        }

        return BadRequest(new { message });
    }

    private static CreatePublicationDto MapCreateDto(CreatePublicationFormRequest request)
    {
        return new CreatePublicationDto
        {
            Title = request.Title,
            Description = request.Description,
            Price = request.Price,
            PropertyTypeId = request.PropertyTypeId,
            Hood = request.Hood,
            Piso = request.Piso,
            Commune = request.Commune,
            Address = request.Address,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Type = request.Type,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = request.IsActive
        };
    }

    private static IEnumerable<IFileUpload>? MapFiles(IEnumerable<IFormFile>? files)
    {
        var uploads = files?
            .Where(file => file.Length > 0)
            .Select(file => (IFileUpload)new FormFileUpload(file))
            .ToList();

        return uploads is { Count: > 0 } ? uploads : null;
    }
}
