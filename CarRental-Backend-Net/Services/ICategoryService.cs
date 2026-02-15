using CarRental_Backend_Net.DTOs;
using CarRental_Backend_Net.Models;

namespace CarRental_Backend_Net.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category?> GetByIdAsync(int id);
        Task<Category> CreateAsync(CategoryCreateDto dto);
        Task<Category?> UpdateAsync(int id, CategoryUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
