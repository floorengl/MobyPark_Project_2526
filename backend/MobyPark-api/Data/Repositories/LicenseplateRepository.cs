using Microsoft.EntityFrameworkCore;

public sealed class LicenseplateRepository : GenericRepository<Licenseplate, long>, ILicenseplateRepository
{
    public LicenseplateRepository(AppDbContext db) : base(db) { }

    public Task<Licenseplate?> GetByPlateTextAsync(string plateText, CancellationToken ct = default)
        => _db.LicensePlates
            .FirstOrDefaultAsync(p => p.LicensePlateName == plateText, ct);

    public Task<List<Licenseplate>> GetAllOrderedAsync(CancellationToken ct = default)
        => _db.LicensePlates
            .OrderBy(x => x.LicensePlateName)
            .AsNoTracking()
            .ToListAsync(ct);

    public Task<List<LicenseplateDto>> GetAllDtosAsync(CancellationToken ct = default)
        => _db.LicensePlates
            .OrderBy(x => x.LicensePlateName)
            .AsNoTracking()
            .Select(x => new LicenseplateDto { LicensePlateName = x.LicensePlateName })
            .ToListAsync(ct);

    public async Task<LicenseplateDto?> GetDtoByPlateTextAsync(string plateText, CancellationToken ct = default)
    {
        var entity = await _db.LicensePlates
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.LicensePlateName == plateText, ct);

        return entity == null
            ? null
            : new LicenseplateDto { LicensePlateName = entity.LicensePlateName };
    }

    public async Task ResetIdentityIfEmptyAsync(CancellationToken ct = default)
    {
        if (!await _db.LicensePlates.AnyAsync(ct))
        {
            // Make sure table/column names match your DB schema:
            await _db.Database.ExecuteSqlRawAsync(
                "SELECT setval(pg_get_serial_sequence('public.licenseplates','id'), 1, false);",
                ct);
        }
    }
}
