public interface ILicenseplateRepository : IGenericRepository<Licenseplate, long>
{
    Task<Licenseplate?> GetByPlateTextAsync(string plateText, CancellationToken ct = default);

    Task<List<Licenseplate>> GetAllOrderedAsync(CancellationToken ct = default);

    Task<List<LicenseplateDto>> GetAllDtosAsync(CancellationToken ct = default);

    Task<LicenseplateDto?> GetDtoByPlateTextAsync(string plateText, CancellationToken ct = default);

    Task ResetIdentityIfEmptyAsync(CancellationToken ct = default);
}
