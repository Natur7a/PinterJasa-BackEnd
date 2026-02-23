using Microsoft.EntityFrameworkCore;
using PinterJasa.API.Data;
using PinterJasa.API.DTOs.Services;
using PinterJasa.API.Models;
using PinterJasa.API.Services.Interfaces;

namespace PinterJasa.API.Services;

public class ServiceService : IServiceService
{
    private readonly AppDbContext _db;

    public ServiceService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<ServiceResponse>> GetActiveServicesAsync(Guid? categoryId)
    {
        var query = _db.Services
            .Include(s => s.Category)
            .Where(s => s.Status == "active");

        if (categoryId.HasValue)
            query = query.Where(s => s.CategoryId == categoryId.Value);

        var services = await query.ToListAsync();
        return services.Select(MapToResponse);
    }

    public async Task<ServiceResponse> GetByIdAsync(Guid id)
    {
        var service = await _db.Services.Include(s => s.Category).FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new KeyNotFoundException($"Service {id} not found.");
        return MapToResponse(service);
    }

    public async Task<ServiceResponse> CreateAsync(Guid providerId, CreateServiceRequest request)
    {
        var provider = await _db.Providers.FirstOrDefaultAsync(p => p.UserId == providerId)
            ?? throw new KeyNotFoundException("Provider profile not found.");

        var category = await _db.Categories.FindAsync(request.CategoryId)
            ?? throw new KeyNotFoundException($"Category {request.CategoryId} not found.");

        var service = new Service
        {
            ProviderId = provider.Id,
            CategoryId = request.CategoryId,
            Title = request.Title,
            Description = request.Description,
            Price = request.Price,
            PriceUnit = request.PriceUnit
        };

        _db.Services.Add(service);
        await _db.SaveChangesAsync();

        service.Category = category;
        return MapToResponse(service);
    }

    public async Task<IEnumerable<ServiceResponse>> GetMyServicesAsync(Guid providerId)
    {
        var provider = await _db.Providers.FirstOrDefaultAsync(p => p.UserId == providerId)
            ?? throw new KeyNotFoundException("Provider profile not found.");

        var services = await _db.Services
            .Include(s => s.Category)
            .Where(s => s.ProviderId == provider.Id)
            .ToListAsync();

        return services.Select(MapToResponse);
    }

    private static ServiceResponse MapToResponse(Service s) => new()
    {
        Id = s.Id,
        ProviderId = s.ProviderId,
        CategoryId = s.CategoryId,
        CategoryName = s.Category?.Name ?? string.Empty,
        Title = s.Title,
        Description = s.Description,
        Price = s.Price,
        PriceUnit = s.PriceUnit,
        Status = s.Status,
        CreatedAt = s.CreatedAt
    };
}
