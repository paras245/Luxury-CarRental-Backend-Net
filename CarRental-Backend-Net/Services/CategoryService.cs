using Microsoft.EntityFrameworkCore;
using CarRental_Backend_Net.Data;
using CarRental_Backend_Net.DTOs;
using CarRental_Backend_Net.Models;

namespace CarRental_Backend_Net.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<CategoryService> _logger;
        private readonly IWebHostEnvironment _env;

        public CategoryService(ApplicationDbContext db, ILogger<CategoryService> logger, IWebHostEnvironment env)
        {
            _db = db;
            _logger = logger;
            _env = env;
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all categories.");
                return await _db.Categories.Include(c => c.Vehicles).AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching categories.");
                throw;
            }
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            try
            {
                 _logger.LogInformation("Fetching category with ID {Id}", id);
                return await _db.Categories.Include(c => c.Vehicles).FirstOrDefaultAsync(c => c.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching category {Id}.", id);
                throw;
            }
        }

        public async Task<Category> CreateAsync(CategoryCreateDto dto)
        {
            try
            {
                _logger.LogInformation("Creating new category: {Name}", dto.Name);
                var category = new Category { Name = dto.Name, Description = dto.Description };

                if (dto.Image != null)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.Image.FileName)}";
                    var uploadPath = Path.Combine(_env.WebRootPath, "uploads", "categories");
                    
                    if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                    using (var stream = new FileStream(Path.Combine(uploadPath, fileName), FileMode.Create))
                    {
                        await dto.Image.CopyToAsync(stream);
                    }

                    category.ImagePath = Path.Combine("uploads", "categories", fileName).Replace("\\", "/");
                }

                _db.Categories.Add(category);
                await _db.SaveChangesAsync();
                _logger.LogInformation("Category created successfully with ID {Id}", category.Id);
                
                return category;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category {Name}.", dto.Name);
                throw;
            }
        }

        public async Task<Category?> UpdateAsync(int id, CategoryUpdateDto dto)
        {
            try
            {
                var cat = await _db.Categories.FindAsync(id);
                if (cat == null) return null;

                cat.Name = dto.Name;
                cat.Description = dto.Description;
                
                await _db.SaveChangesAsync();
                _logger.LogInformation("Category {Id} updated.", id);
                return cat;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category {Id}.", id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var cat = await _db.Categories.FindAsync(id);
                if (cat == null) return false;

                _db.Categories.Remove(cat);
                await _db.SaveChangesAsync();
                _logger.LogInformation("Category {Id} deleted.", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category {Id}.", id);
                throw;
            }
        }
    }
}
