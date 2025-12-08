using Microsoft.EntityFrameworkCore;
using MobyPark_api.Dtos;

public interface ILicenseplateService
{
    Task<(long, string)> LicenseplatesAsync(CheckInDto dto, CancellationToken cto);
    Task DeleteAsync(string plateText, CancellationToken ct);
    Task<IReadOnlyList<LicenseplateDto>> GetAllAsync(CancellationToken ct);
    Task<LicenseplateDto?> GetByPlateAsync(string plate, CancellationToken ct);
    Task<(bool legal, string reason)> IsCheckInLegal(CheckInDto dto);

}