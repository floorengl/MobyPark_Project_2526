using Microsoft.EntityFrameworkCore;

public sealed class SessionService : ISessionService
{
    private readonly AppDbContext _db;
    public SessionService(AppDbContext db) => _db = db;

    public async Task<long> StartForPlateAsync(long parkingLotId, long licensePlateId, CancellationToken ct)
    {
        var open = await _db.Sessions
            .FirstOrDefaultAsync(s => s.ParkingLotId == parkingLotId
                                && s.LicensePlateId == licensePlateId
                                && s.Stopped == null, ct);
        if (open != null) return open.Id;

        var plateText = await _db.LicensePlates
            .Where(p => p.Id == licensePlateId)
            .Select(p => p.LicensePlateName)
            .FirstAsync(ct); // we just created it, so it's there

        var session = new Session
        {
            ParkingLotId = parkingLotId,
            LicensePlateId = licensePlateId,
            PlateText = plateText!,             // snapshot here
            Started = DateTime.UtcNow
        };

        _db.Sessions.Add(session);
        await _db.SaveChangesAsync(ct);
        return session.Id;
    }

    public async Task StopOpenForPlateAsync(long parkingLotId, long licensePlateId, CancellationToken ct)
    {
        var session = await _db.Sessions
            .FirstOrDefaultAsync(s => s.ParkingLotId == parkingLotId
                                   && s.LicensePlateId == licensePlateId
                                   && s.Stopped == null, ct);
        if (session == null) return; // nothing to stop

        session.Stopped = DateTime.UtcNow;
        var minutes = Math.Max(1, (int)Math.Ceiling((session.Stopped.Value - session.Started).TotalMinutes));
        session.DurationMinutes = (short)Math.Min(short.MaxValue, minutes);
        session.Cost = null; // pricing later

        await _db.SaveChangesAsync(ct);
    }
}
