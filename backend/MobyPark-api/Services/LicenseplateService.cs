using Microsoft.EntityFrameworkCore;
using MobyPark_api.Dtos;


public sealed class LicenseplateService : ILicenseplateService
{
    private readonly AppDbContext _db;
    private readonly ILicenseplateRepository _plates;
    private readonly ISessionService _sessions;
    private readonly IReservationService _reservations;

    public LicenseplateService(
        AppDbContext db,
        ILicenseplateRepository plates,
        ISessionService sessions,
        IReservationService reservations)
        => (_db, _plates, _sessions, _reservations) = (db, plates, sessions, reservations);

    // POST Licenseplate / Start session
    public async Task<long> LicenseplatesAsync(CheckInDto dto, CancellationToken ct)
    {
        var lot = await _db.ParkingLots.FindAsync(new object?[] { dto.ParkingLotId }, ct);
        if (lot == null) return 0;
        // Check occupancy and reservation.
        int occupied = await _db.Sessions.CountAsync(s => s.ParkingLotId == lot.Id && s.Stopped == null, ct);
        var reservation = await _reservations.GetActiveReservation(dto.LicensePlateName, DateTime.UtcNow);

        if (reservation != null)
        {
            if (occupied > lot.Capacity) return 0;
            await _reservations.ConsumeReservation(reservation.Id);
        }
        else
        {
            if (await _reservations.WillParkingLotOverflow(
                    DateTime.UtcNow,
                    DateTime.UtcNow.AddHours(4),
                    dto.ParkingLotId + occupied))
            {
                return 0;
            }
        }
        // Creating plate.
        var plate = new Licenseplate { LicensePlateName = dto.LicensePlateName };
        await _plates.AddAsync(plate, ct);
        await _plates.SaveChangesAsync(ct);
        // Starting session.
        await _sessions.StartForPlateAsync(dto.ParkingLotId, plate.Id, ct);
        return plate.Id;
    }

    // DELETE Licenseplate / Stop Session
    public async Task DeleteAsync(string plateText, CancellationToken ct)
    {
        var plate = await _plates.GetByPlateTextAsync(plateText, ct);
        if (plate == null) return;
        // Finding parkinglotId where plate has an open session
        long lotId = await _db.Sessions
            .Where(s => s.Stopped == null && s.LicensePlate.LicensePlateName == plateText)
            .Select(s => s.ParkingLotId)
            .FirstOrDefaultAsync(ct);
        await _sessions.StopOpenForPlateAsync(lotId, plate.Id, ct);
        // Remove plate.
        _plates.Remove(plate);
        await _plates.SaveChangesAsync(ct);
        await _plates.ResetIdentityIfEmptyAsync(ct);
    }

    // GET ALL Licenseplates
    public async Task<IReadOnlyList<LicenseplateDto>> GetAllAsync(CancellationToken ct)
        => await _plates.GetAllDtosAsync(ct);

    // GET ONE Licenseplates
    public Task<LicenseplateDto?> GetByPlateAsync(string plateText, CancellationToken ct)
        => _plates.GetDtoByPlateTextAsync(plateText, ct);
}
