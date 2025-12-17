using Microsoft.EntityFrameworkCore;

public sealed class LicenseplateRepository : GenericRepository<Licenseplate, long>, ILicenseplateRepository
{
    public LicenseplateRepository(AppDbContext db) : base(db) { }

    public Task<Licenseplate?> GetByPlateTextAsync(string plateText, CancellationToken ct = default)
        => _db.LicensePlates.FirstOrDefaultAsync(p => p.LicensePlateName == plateText, ct);

    public Task<List<Licenseplate>> GetAllOrderedAsync(CancellationToken ct = default)
        => _db.LicensePlates
            .OrderBy(x => x.LicensePlateName)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task ResetIdentityIfEmptyAsync(CancellationToken ct = default)
    {
        if (!await _db.LicensePlates.AnyAsync(ct))
        {
            await _db.Database.ExecuteSqlRawAsync(
                "SELECT setval(pg_get_serial_sequence('public.licenseplates','id'), 1, false);",
                ct);
        }
    }
}

