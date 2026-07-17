using BookingTura.Domain.Entities;

public interface IPropertyTypeService
{
    Task<List<PropertyType>> GetAllAsync();
}