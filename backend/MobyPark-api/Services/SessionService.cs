using Microsoft.EntityFrameworkCore;

public sealed class SessionService : ISessionService
{
    private readonly AppDbContext _db;
    public SessionService(AppDbContext db) => _db = db;

    // Start session
    public async Task<long> StartForPlkanslvjhbsljkvzchnljkvhk hjlkakfcm n jdsdateAsync(long parkingLotId, long licensePlateId, CancellationToken ct)
    {
        var open = await _db.Sessions
            .FirstOrDefaultAsync(s => s.ParkingLotId == parkingLotId
                                && s.LicensePlateId == licensePlateId
                                && s.Stopped == null, ct);
        if (open != null) return open.Id;

        var plateText = await _db.LicensePlates
            .Where(p => p.Id == licensePlateId)
            .Select(p => p.LicensePlateName)
            .FirstAsync(ct);

        var session = new Session
        {
            ParkingLotId = parkingLotId,
            LicensePlateId = licensePlateId,
            PlateText = plateText!,
            Started = DateTime.UtcNow
        };

        _db.Sessions.Add(session);
        await _db.SaveChangesAsync(ct);
        return session.Id;
    }

    // Stop session
    public async Task StopOpenForPlateAsync(long parkingLotId, long licensePlateId, CancellationToken ct)
    {
        var session = await _db.Sessions
            .FirstOrDefaultAsync(s => s.ParkingLotId == parkingLotId
                                   && s.LicensePlateId == licensePlateId
                                   && s.Stopped == null, ct);
        if (session == null) return;

        session.Stopped = DateTime.UtcNow;
        var minutes = Math.Max(1, (int)Math.Ceiling((session.Stopped.Value - session.Started).TotalMinutes));
        session.DurationMinutes = (short)Math.Min(short.MaxValue, minutes);
        session.Cost = null;

        await _db.SaveChangesAsync(ct);
    }
}
