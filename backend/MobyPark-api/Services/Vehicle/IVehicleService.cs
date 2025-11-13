using MobyPark_api.Dtos.Vehicle;

namespace MobyPark_api.Services.VehicleService
{
    public interface IVehicleService
    {
        Task<IEnumerable<VehicleDto>> GetAllVehicles();
        Task<IEnumerable<VehicleDto>> GetUserVehiclesAsync(long userId);
        Task<IEnumerable<VehicleDto>> GetVehiclesByUsernameAsync(string username);
        Task<VehicleDto?> GetByIdAsync(long id, long userId);
        Task<VehicleDto?> CreateAsync(VehicleDto dto, long userId);
        Task<VehicleDto?> UpdateAsync(long id, VehicleDto dto, long userId);
        Task<bool> DeleteAsync(long id, long userId);
    }
}
