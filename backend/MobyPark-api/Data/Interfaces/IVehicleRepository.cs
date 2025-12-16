using MobyPark_api.Data.Models;

public interface IVehicleRepository : IGenericRepository<Vehicle, long>
{
    Task<bool> LicensePlateExistsAsync(string licensePlate, CancellationToken ct = default);
    Task<Vehicle?> GetByIdForUserAsync(long id, long userId, CancellationToken ct = default);
    Task<List<Vehicle>> GetAllAsyncWithUser(CancellationToken ct = default);
    Task<List<Vehicle>> GetByUserIdAsync(long userId, CancellationToken ct = default);
    Task<List<Vehicle>> GetByUsernameAsync(string username, CancellationToken ct = default);
}
