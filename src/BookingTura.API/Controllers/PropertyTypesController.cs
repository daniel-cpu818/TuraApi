using Microsoft.AspNetCore.Mvc;
using BookingTura.Application.Interfaces;

namespace BookingTura.API.Controllers;

[ApiController]
[Route("api/property-types")]
public class PropertyTypesController : ControllerBase
{
    private readonly IPropertyTypeService _service;

    public PropertyTypesController(IPropertyTypeService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var types = await _service.GetAllAsync();
        return Ok(types);
    }
}