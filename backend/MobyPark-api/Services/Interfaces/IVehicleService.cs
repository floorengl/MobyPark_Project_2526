using MobyPark_api.Dtos.Vehicle;

public interface IVehicleService
{
    Task<IEnumerable<VehicleDto>> GetAllVehicles(CancellationToken ct = default);
    Task<VehicleDto?> GetByIdAsync(long id, long userId, CancellationToken ct = default);
    Task<IEnumerable<VehicleDto>> GetUserVehiclesAsync(long userId, CancellationToken ct = default);
    Task<IEnumerable<VehicleDto>> GetVehiclesByUsernameAsync(string username, CancellationToken ct = default);
    Task<VehicleDto?> CreateAsync(VehicleDto dto, long userId, CancellationToken ct = default);
    Task<VehicleDto?> UpdateAsync(long id, VehicleDto dto, long userId, CancellationToken ct = default);
    Task<bool> DeleteAsync(long id, long userId, CancellationToken ct = default);
}
