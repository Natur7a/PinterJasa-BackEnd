using PinterJasa.API.Models;

namespace PinterJasa.API.Services.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<Category>> GetAllActiveAsync();
    Task<Category> GetByIdAsync(Guid id);
}
