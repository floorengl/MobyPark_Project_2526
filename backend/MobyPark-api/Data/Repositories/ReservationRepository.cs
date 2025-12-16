using MobyPark_api.Data.Models;
using Microsoft.EntityFrameworkCore;

public sealed class ReservationRepository : GenericRepository<Reservation, Guid>, IReservationRepository
{
    public ReservationRepository(AppDbContext db) : base(db) { }

    public Task<List<Reservation>> GetAllNoTrackingAsync(CancellationToken ct = default)
        => _db.Reservations.AsNoTracking().ToListAsync(ct);

    public Task<Reservation?> GetByIdNoTrackingAsync(Guid id, CancellationToken ct = default)
        => _db.Reservations.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<Reservation?> GetActiveReservationEntityAsync(string licensePlate, DateTime time, CancellationToken ct = default)
        => _db.Reservations
            .FirstOrDefaultAsync(r =>
                r.LicensePlate == licensePlate &&
                r.StartTime < time &&
                r.EndTime > time &&
                r.Status == ReservationStatus.UnUsed, ct);

    public async Task<(DateTime start, DateTime end)[]> GetOverlappingUnusedIntervalsAsync(
        long parkingLotId, DateTime start, DateTime end, CancellationToken ct = default)
    {
        return await _db.Reservations
            .Where(r => r.ParkingLotId == parkingLotId)
            .Where(r => r.Status == ReservationStatus.UnUsed)
            .Where(r => start < r.EndTime && r.StartTime < end) // overlaps
            .AsNoTracking()
            .Select(r => new ValueTuple<DateTime, DateTime>(r.StartTime, r.EndTime))
            .ToArrayAsync(ct);
    }
}
