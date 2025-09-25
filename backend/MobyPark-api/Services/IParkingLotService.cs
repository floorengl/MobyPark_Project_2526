using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MobyPark_api.DTOs;

namespace MobyPark_api.Services
{
    public interface IParkingLotService
    {
        Task<ParkingLotReadDto> CreateAsync(CreateParkingLotDto dto, CancellationToken ct);
        Task<ParkingLotReadDto?> GetAsync(long id, CancellationToken ct);
        Task<IReadOnlyList<ParkingLotReadDto>> ListAsync(CancellationToken ct);
        Task<bool> UpdateAsync(long id, UpdateParkingLotDto dto, CancellationToken ct);
        Task<bool> DeleteAsync(long id, CancellationToken ct);
    }
}
