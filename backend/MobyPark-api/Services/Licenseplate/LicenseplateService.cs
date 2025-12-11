using Microsoft.EntityFrameworkCore;

public sealed class LicenseplateService : ILicenseplateService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _cfg;
    private readonly ISessionService _sessions;

    public LicenseplateService(AppDbContext db, IConfiguration cfg, ISessionService sessions)
        => (_db, _cfg, _sessions) = (db, cfg, sessions);


    // POST Licenseplate / Start session
    public async Task<long> LicenseplatesAsync(LicenseplateDto dto, CancellationToken cto)
    {
        var plate = new Licenseplate { LicensePlateName = dto.LicensePlateName };
        _db.LicensePlates.Add(plate);
        await _db.SaveChangesAsync(cto);
        var parkingLotId = _cfg.GetValue<long>("DefaultParkingLotId", 1L);
        await _sessions.StartForPlateAsync(parkingLotId, plate.Id, cto);
        return plate.Id;
    }

    // DELETE Licenseplate / Stop Session
    public async Task DeleteAsync(string plateText, CancellationToken ct)
    {
        var plate = await _db.LicensePlates
            .FirstOrDefaultAsync(p => p.LicensePlateName == plateText, ct);

        if (plate == null) return;

        var parkingLotId = _cfg.GetValue<long>("DefaultParkingLotId", 1L);
        await _sessions.StopOpenForPlateAsync(parkingLotId, plate.Id, ct);

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
