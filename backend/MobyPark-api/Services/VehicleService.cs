using MobyPark_api.Data.Models;
using MobyPark_api.Dtos.Vehicle;

public sealed class VehicleService : IVehicleService
{
    private readonly IVehicleRepository _repo;

    public VehicleService(IVehicleRepository repo) => _repo = repo;

    private static VehicleDto ToDto(Vehicle v) => new VehicleDto
    {
        Id = v.Id,
        LicensePlate = v.LicensePlate,
        Make = v.Make,
        Model = v.Model,
        Color = v.Color,
        Year = v.Year,
        UserId = v.UserId,
        CreatedAt = v.CreatedAt,
    };

    public async Task<IEnumerable<VehicleDto>> GetAllVehicles(CancellationToken ct = default)
        => (await _repo.GetAllAsync(ct)).Select(ToDto);

    public async Task<VehicleDto?> GetByIdAsync(long id, long userId, CancellationToken ct = default)
    {
        var vehicle = await _repo.GetByIdForUserAsync(id, userId, ct);
        return vehicle is null ? null : ToDto(vehicle);
    }

    public async Task<IEnumerable<VehicleDto>> GetUserVehiclesAsync(long userId, CancellationToken ct = default)
        => (await _repo.GetByUserIdAsync(userId, ct)).Select(ToDto);

    public async Task<IEnumerable<VehicleDto>> GetVehiclesByUsernameAsync(string username, CancellationToken ct = default)
        => (await _repo.GetByUsernameAsync(username, ct)).Select(ToDto);

    public async Task<VehicleDto?> CreateAsync(VehicleDto dto, long userId, CancellationToken ct = default)
    {
        if (await _repo.LicensePlateExistsAsync(dto.LicensePlate, ct))
            return null;

        var vehicle = new Vehicle
        {
            LicensePlate = dto.LicensePlate,
            Make = dto.Make,
            Model = dto.Model,
            Color = dto.Color,
            Year = dto.Year,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(vehicle, ct);
        await _repo.SaveChangesAsync(ct);

        return ToDto(vehicle);
    }

    public async Task<VehicleDto?> UpdateAsync(long id, VehicleDto dto, long userId, CancellationToken ct = default)
    {
        var vehicle = await _repo.GetByIdForUserAsync(id, userId, ct);
        if (vehicle == null || vehicle.UserId != userId) return null;

        if (!string.Equals(vehicle.LicensePlate, dto.LicensePlate, StringComparison.OrdinalIgnoreCase)
            && await _repo.LicensePlateExistsAsync(dto.LicensePlate, ct))
        {
            return null;
        }

        vehicle.LicensePlate = dto.LicensePlate;
        vehicle.Make = dto.Make;
        vehicle.Model = dto.Model;
        vehicle.Color = dto.Color;
        vehicle.Year = dto.Year;

        await _repo.SaveChangesAsync(ct);
        return ToDto(vehicle);
    }

    public async Task<bool> DeleteAsync(long id, long userId, CancellationToken ct = default)
    {
        var vehicle = await _repo.GetByIdForUserAsync(id, userId, ct);
        if (vehicle is null) return false;

        _repo.Remove(vehicle);
        await _repo.SaveChangesAsync(ct);
        return true;
    }
}
