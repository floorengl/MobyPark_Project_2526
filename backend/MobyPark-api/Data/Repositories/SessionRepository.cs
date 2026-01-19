using Microsoft.EntityFrameworkCore;
using MobyPark_api.Data.Models;

public sealed class SessionRepository : GenericRepository<Session, long>, ISessionRepository
{
    public SessionRepository(AppDbContext db) : base(db) { }

    // gets open session for a given plate in a given parking lot
    public Task<Session?> GetOpenForPlateAsync(long parkingLotId, long licensePlateId, CancellationToken ct = default)
        => _db.Sessions
            .FirstOrDefaultAsync(s => s.ParkingLotId == parkingLotId
                                  && s.LicensePlateId == licensePlateId
                                  && s.Stopped == null, ct);

    // gets license plate text by id
    public Task<string> GetPlateTextAsync(long licensePlateId, CancellationToken ct = default)
        => _db.LicensePlates
            .Where(p => p.Id == licensePlateId)
            .Select(p => p.LicensePlateName)
            .FirstAsync(ct);

    // gets reservation for a given plate in a given parking lot
    public Task<List<Reservation>> GetReservationForPlateAsync(long parkingLotId, string licensePlate, CancellationToken ct = default)
        => _db.Reservations
            .Where(r => r.ParkingLotId == parkingLotId
                     && r.LicensePlate == licensePlate)
            .ToListAsync(ct);

    public Task<ParkingLot?> GetParkingLotAsync(long parkingLotId, CancellationToken ct = default)
        => _db.ParkingLots
            .FirstOrDefaultAsync(p => p.Id == parkingLotId, ct);
}
