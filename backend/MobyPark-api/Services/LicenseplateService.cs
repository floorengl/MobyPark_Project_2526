using Microsoft.EntityFrameworkCore;

public sealed class LicenseplateService : ILicenseplateService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _cfg;
    private readonly ISessionService _sessions;

    public LicenseplateService(AppDbContext db, IConfiguration cfg, ISessionService sessions)
        => (_db, _cfg, _sessions) = (db, cfg, sessions);

    // POST /licenseplate (unchanged signature)
    public async Task<long> LicenseplatesAsync(LicenseplateDto dto, CancellationToken cto)
    {

        // upsert or always create? — keep your current behavior: create a row
        var plate = new Licenseplate { LicensePlateName = dto.LicensePlateName };
        _db.LicensePlates.Add(plate);
        await _db.SaveChangesAsync(cto);

        // start session in default lot
        var parkingLotId = _cfg.GetValue<long>("DefaultParkingLotId", 1L);
        await _sessions.StartForPlateAsync(parkingLotId, plate.Id, cto);

        return plate.Id;
    }

    // NEW: delete + stop session
    public async Task DeleteAsync(string plateText, CancellationToken ct)
    {
        var plate = await _db.LicensePlates
            .FirstOrDefaultAsync(p => p.LicensePlateName == plateText, ct);

        if (plate == null) return;

        var parkingLotId = _cfg.GetValue<long>("DefaultParkingLotId", 1L);
        await _sessions.StopOpenForPlateAsync(parkingLotId, plate.Id, ct);

        _db.LicensePlates.Remove(plate);
        await _db.SaveChangesAsync(ct);
    }
}
