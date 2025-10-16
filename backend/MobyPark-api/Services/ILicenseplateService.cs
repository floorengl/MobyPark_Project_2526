using Microsoft.EntityFrameworkCore;

public interface ILicenseplateService
{
    Task<long> LicenseplatesAsync(LicenseplateDto dto, CancellationToken cto);
    Task DeleteAsync(string plateText, CancellationToken ct);
    Task<IReadOnlyList<LicenseplateDto>> GetAllAsync(CancellationToken ct);
    Task<LicenseplateDto?> GetByPlateAsync(string plate, CancellationToken ct);

}