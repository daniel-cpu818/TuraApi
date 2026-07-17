using BookingTura.API.Files;
using BookingTura.API.Models.Properties;
using BookingTura.Application.DTOs.Properties;
using BookingTura.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookingTura.API.Controllers;

[ApiController]
[Route("api/properties")]
public class PropertyController : ControllerBase
{
    private readonly IPropertyService _service;

    public PropertyController(IPropertyService service)
    {
        _service = service;
    }

    // [Authorize]
    [HttpPost]
    [Consumes("application/json")]
    public async Task<IActionResult> Create([FromBody] CreatePropertyDto dto)
    {
        try
        {
            var property = await _service.CreateAsync(dto);
            return Ok(property);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    // [Authorize]
    [HttpPost("with-images")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateWithImages([FromForm] CreatePropertyFormRequest request)
    {
        if (request.PropertyTypeId == Guid.Empty)
        {
            return BadRequest(new
            {
                message = "El campo propertyTypeId es obligatorio en form-data y debe ser un GUID válido."
            });
        }

        try
        {
            var property = await _service.CreateAsync(
                MapCreateDto(request),
                MapFiles(request.Images));

            return Ok(property);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
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
        var property = await _service.GetByIdAsync(id);
        return property == null ? NotFound() : Ok(property);
    }

    // [Authorize]
    [HttpPut("{id:guid}")]
    [Consumes("application/json")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePropertyDto dto)
    {
        try
        {
            var property = await _service.UpdateAsync(id, dto);
            return property == null ? NotFound() : Ok(property);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    // [Authorize]
    [HttpPut("{id:guid}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateWithImages(Guid id, [FromForm] UpdatePropertyFormRequest request)
    {
        if (request.PropertyTypeId.HasValue && request.PropertyTypeId.Value == Guid.Empty)
        {
            return BadRequest(new
            {
                message = "Si envías propertyTypeId en form-data, debe ser un GUID válido."
            });
        }

        try
        {
            var property = await _service.UpdateAsync(
                id,
                MapUpdateDto(request),
                MapFiles(request.Images),
                request.ReplaceImages);

            return property == null ? NotFound() : Ok(property);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    // [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id);
        return result ? Ok() : NotFound();
    }

    private static CreatePropertyDto MapCreateDto(CreatePropertyFormRequest request)
    {
        return new CreatePropertyDto
        {
            Title = request.Title,
            Description = request.Description,
            Price = request.Price,
            PropertyTypeId = request.PropertyTypeId,
            Hood = request.Hood,
            Piso = request.Piso,
            Commune = request.Commune,
            Address = request.Address
        };
    }

    private static UpdatePropertyDto MapUpdateDto(UpdatePropertyFormRequest request)
    {
        return new UpdatePropertyDto
        {
            Title = request.Title,
            Description = request.Description,
            Price = request.Price,
            PropertyTypeId = request.PropertyTypeId,
            Hood = request.Hood,
            Piso = request.Piso,
            Commune = request.Commune,
            Address = request.Address
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
