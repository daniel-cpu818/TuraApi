using BookingTura.Application.Interfaces;
using BookingTura.Domain.Entities;
using BookingTura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingTura.Infrastructure.Services;

public class PropertyTypeService : IPropertyTypeService
{
    private readonly BookingTuraDbContext _context;

    public PropertyTypeService(BookingTuraDbContext context)
    {
        _context = context;
    }

    public async Task<List<PropertyType>> GetAllAsync()
    {
        return await _context.PropertyTypes.ToListAsync();
    }
}