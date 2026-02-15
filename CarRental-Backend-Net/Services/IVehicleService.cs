using CarRental_Backend_Net.DTOs;
using CarRental_Backend_Net.Models;

namespace CarRental_Backend_Net.Services
{
    public interface IVehicleService
    {
        Task<IEnumerable<Vehicle>> GetAllAsync();
        Task<Vehicle?> GetByIdAsync(int id);
        Task<IEnumerable<Vehicle>> GetOffersAsync();
        Task<IEnumerable<Vehicle>> GetTopSellingAsync();
        Task<Vehicle> CreateAsync(VehicleCreateDto dto);
        Task<Vehicle?> UpdateAsync(int id, VehiclePatchDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
