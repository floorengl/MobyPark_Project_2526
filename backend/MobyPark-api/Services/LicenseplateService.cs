using Microsoft.EntityFrameworkCore;
using MobyPark_api.Dtos;

public sealed class LicenseplateService : ILicenseplateService
{
    private readonly AppDbContext _db;
    private readonly ISessionService _sessions;
    private readonly IReservationService _reservations;

    public LicenseplateService(AppDbContext db, ISessionService sessions, IReservationService reservations)
        => (_db, _sessions, _reservations) = (db, sessions, reservations);


    // POST Licenseplate / Start session
    public async Task<long> LicenseplatesAsync(CheckInDto dto, CancellationToken cto)
    {
        // input validation
        var lot = await _db.ParkingLots.FindAsync(dto.ParkingLotId);
        if (lot == null) return 0;

        int occupied = await _db.Sessions.CountAsync(ses => ses.ParkingLotId == lot.Id && ses.Stopped == null);
        var reservation = await _reservations.GetActiveReservation(dto.LicensePlateName, DateTime.UtcNow);

        // check if space is available accounting for a possible reservation
        if (reservation != null)
        {
            if (occupied > lot.Capacity)
            {
                return 0;
            }
            else
            {
                await _reservations.ConsumeReservation(reservation.Id);
            }
        }
        else
        {
            if (await _reservations.WillParkingLotOverflow(DateTime.UtcNow, DateTime.UtcNow.AddHours(4), dto.ParkingLotId + occupied))
            {
                return 0;
            }
        }

        var plate = new Licenseplate { LicensePlateName = dto.LicensePlateName };
        _db.LicensePlates.Add(plate);
        await _db.SaveChangesAsync(cto);
        await _sessions.StartForPlateAsync(dto.ParkingLotId, plate.Id, cto);
        return plate.Id;
    }

    // DELETE Licenseplate / Stop Session
    public async Task DeleteAsync(string plateText, CancellationToken ct)
    {
        var plate = await _db.LicensePlates
            .FirstOrDefaultAsync(p => p.LicensePlateName == plateText, ct);

        if (plate == null) return;
        long lotId = await _db.Sessions
            .Where(s => s.Stopped == null && s.LicensePlate.LicensePlateName == plateText)
            .Select(s => s.ParkingLotId)
            .FirstOrDefaultAsync();

        await _sessions.StopOpenForPlateAsync(lotId, plate.Id, ct);

        _db.LicensePlates.Remove(plate);
        await _db.SaveChangesAsync(ct);

        if (!await _db.LicensePlates.AnyAsync(ct))
        {
            await _db.Database.ExecuteSqlRawAsync(
                "SELECT setval(pg_get_serial_sequence('public.licenseplates','id'), 1, false);",
                ct);
        }
    }

    // GET ALL Licenseplates
    public async Task<IReadOnlyList<LicenseplateDto>> GetAllAsync(CancellationToken ct) =>
        await _db.Set<Licenseplate>()
            .AsNoTracking()
            .OrderBy(x => x.LicensePlateName)
            .Select(x => new LicenseplateDto { LicensePlateName = x.LicensePlateName })
            .ToListAsync(ct);

    // GET ONE Licenseplates
    public async Task<LicenseplateDto?> GetByPlateAsync(string plate, CancellationToken ct)
    {
        var key = plate;
        var x = await _db.Set<Licenseplate>()
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.LicensePlateName == key, ct);
        return x is null ? null : new LicenseplateDto { LicensePlateName = x.LicensePlateName };
    }
}
