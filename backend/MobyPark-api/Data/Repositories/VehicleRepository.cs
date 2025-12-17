using Microsoft.EntityFrameworkCore;
using MobyPark_api.Data.Models;

public sealed class VehicleRepository : GenericRepository<Vehicle, long>, IVehicleRepository
{
    public VehicleRepository(AppDbContext db) : base(db) { }

    public Task<bool> LicensePlateExistsAsync(string licensePlate, CancellationToken ct = default)
        => _db.Vehicles.AnyAsync(v => v.LicensePlate == licensePlate, ct);

    public Task<Vehicle?> GetByIdForUserAsync(long id, long userId, CancellationToken ct = default)
        => _db.Vehicles.FirstOrDefaultAsync(v => v.Id == id && v.UserId == userId, ct);

    public Task<List<Vehicle>> GetAllAsyncWithUser(CancellationToken ct = default)
        => _db.Vehicles
            .AsNoTracking()
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync(ct);

    public Task<List<Vehicle>> GetByUserIdAsync(long userId, CancellationToken ct = default)
        => _db.Vehicles
            .AsNoTracking()
            .Where(v => v.UserId == userId)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync(ct);

    public Task<List<Vehicle>> GetByUsernameAsync(string username, CancellationToken ct = default)
        => _db.Vehicles
            .AsNoTracking()
            .Where(v => v.User != null && v.User.Username == username)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync(ct);
}
