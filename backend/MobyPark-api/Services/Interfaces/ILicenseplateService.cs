using Microsoft.EntityFrameworkCore;
using MobyPark_api.Dtos;

public interface ILicenseplateService
{
    Task<long> LicenseplatesAsync(CheckInDto dto, CancellationToken ct);
    Task DeleteAsync(string plateText, CancellationToken ct);
    Task<IReadOnlyList<LicenseplateDto>> GetAllAsync(CancellationToken ct);
    Task<LicenseplateDto?> GetByPlateAsync(string plateText, CancellationToken ct);
}
