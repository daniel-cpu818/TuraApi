using BookingTura.Application.DTOs;
using BookingTura.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BookingTura.API.Controllers;

[ApiController]
[Route("api/location")]
public class LocationController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateLocationDto dto,
        ILocationService service)
    {
        var result = await service.CreateAsync(dto);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(ILocationService service)
    {
        var data = await service.GetAllAsync();
        return Ok(data);
    }
}