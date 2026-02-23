using Microsoft.EntityFrameworkCore;
using PinterJasa.API.Data;
using PinterJasa.API.Models;
using PinterJasa.API.Services.Interfaces;

namespace PinterJasa.API.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _db;

    public CategoryService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Category>> GetAllActiveAsync()
    {
        return await _db.Categories.Where(c => c.IsActive).ToListAsync();
    }

    public async Task<Category> GetByIdAsync(Guid id)
    {
        return await _db.Categories.FindAsync(id)
            ?? throw new KeyNotFoundException($"Category {id} not found.");
    }
}
