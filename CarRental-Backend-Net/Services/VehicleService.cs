using Microsoft.EntityFrameworkCore;
using CarRental_Backend_Net.Data;
using CarRental_Backend_Net.DTOs;
using CarRental_Backend_Net.Models;
using System.Text.Json;

namespace CarRental_Backend_Net.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<VehicleService> _logger;
        private readonly IWebHostEnvironment _env;

        public VehicleService(ApplicationDbContext db, ILogger<VehicleService> logger, IWebHostEnvironment env)
        {
            _db = db;
            _logger = logger;
            _env = env;
        }

        public async Task<IEnumerable<Vehicle>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all vehicles.");
                return await _db.Vehicles.Include(v => v.Category).AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching vehicles.");
                throw;
            }
        }

        public async Task<Vehicle?> GetByIdAsync(int id)
        {
            try
            {
                return await _db.Vehicles.Include(v => v.Category).FirstOrDefaultAsync(v => v.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching vehicle {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Vehicle>> GetOffersAsync()
        {
            try
            {
                return await _db.Vehicles.Where(v => v.IsOffer).AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Error fetching offers.");
                throw;
            }
        }

        public async Task<IEnumerable<Vehicle>> GetTopSellingAsync()
        {
            try
            {
                return await _db.Vehicles.Where(v => v.IsTopSelling).AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching top selling vehicles.");
                throw;
            }
        }

        public async Task<Vehicle> CreateAsync(VehicleCreateDto dto)
        {
            try
            {
                _logger.LogInformation("Creating vehicle {Name}", dto.Name);
                
                var vehicle = new Vehicle
                {
                    Name = dto.Name,
                    Type = dto.Type,
                    CategoryId = dto.CategoryId,
                    PricePerDay = dto.PricePerDay,
                    Speed = dto.Speed,
                    Description = dto.Description,
                    IsOffer = dto.IsOffer,
                    OfferPercentage = dto.OfferPercentage,
                    IsTopSelling = dto.IsTopSelling,
                    AvailableForRent = dto.AvailableForRent
                };

                // Image Handling
                var imagePaths = new List<string>();
                
                // Regex to replace invalid characters with underscore
                string safeName = System.Text.RegularExpressions.Regex.Replace(dto.Name, @"[^a-zA-Z0-9\-]", "_");
                var uploadPath = Path.Combine(_env.WebRootPath, "uploads", "vehicles", safeName);
                
                if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                // Helper local function to save file
                async Task SaveImage(IFormFile? image)
                {
                    if (image != null && image.Length > 0)
                    {
                        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                        var filePath = Path.Combine(uploadPath, fileName);
                        
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }
                        imagePaths.Add(Path.Combine("uploads", "vehicles", safeName, fileName).Replace("\\", "/"));
                    }
                }

                await SaveImage(dto.Image1);
                await SaveImage(dto.Image2);
                await SaveImage(dto.Image3);

                vehicle.ImagePaths = JsonSerializer.Serialize(imagePaths);

                _db.Vehicles.Add(vehicle);
                await _db.SaveChangesAsync();
                
                return vehicle;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating vehicle {Name}", dto.Name);
                throw;
            }
        }

        public async Task<Vehicle?> UpdateAsync(int id, VehiclePatchDto dto)
        {
            try
            {
                var vehicle = await _db.Vehicles.FindAsync(id);
                if (vehicle == null) return null;

                if (dto.PricePerDay.HasValue) vehicle.PricePerDay = dto.PricePerDay.Value;
                if (dto.IsOffer.HasValue) vehicle.IsOffer = dto.IsOffer.Value;
                if (dto.OfferPercentage.HasValue) vehicle.OfferPercentage = dto.OfferPercentage.Value;
                if (dto.IsTopSelling.HasValue) vehicle.IsTopSelling = dto.IsTopSelling.Value;
                if (dto.AvailableForRent.HasValue) vehicle.AvailableForRent = dto.AvailableForRent.Value;

                await _db.SaveChangesAsync();
                return vehicle;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vehicle {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var vehicle = await _db.Vehicles.FindAsync(id);
                if (vehicle == null) return false;

                // Delete Folder
                var safeName = string.Join("_", vehicle.Name.Split(Path.GetInvalidFileNameChars()));
                var path = Path.Combine(_env.WebRootPath, "uploads", "vehicles", safeName);
                if (Directory.Exists(path)) Directory.Delete(path, true);

                _db.Vehicles.Remove(vehicle);
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting vehicle {Id}", id);
                throw;
            }
        }
    }
}
