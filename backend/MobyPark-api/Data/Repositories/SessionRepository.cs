using Microsoft.EntityFrameworkCore;
using MobyPark_api.Data.Models;

public sealed class SessionRepository : GenericRepository<Session, long>, ISessionRepository
{
    public SessionRepository(AppDbContext db) : base(db) { }

    public Task<Session?> GetOpenForPlateAsync(long parkingLotId, long licensePlateId, CancellationToken ct = default)
        => _db.Sessions
            .FirstOrDefaultAsync(s => s.ParkingLotId == parkingLotId
                                  && s.LicensePlateId == licensePlateId
                                  && s.Stopped == null, ct);

    public Task<string> GetPlateTextAsync(long licensePlateId, CancellationToken ct = default)
        => _db.LicensePlates
            .Where(p => p.Id == licensePlateId)
            .Select(p => p.LicensePlateName)
            .FirstAsync(ct);

    public Task<ParkingLot?> GetParkingLotAsync(long parkingLotId, CancellationToken ct = default)
        => _db.ParkingLots
            .FirstOrDefaultAsync(p => p.Id == parkingLotId, ct);
}
