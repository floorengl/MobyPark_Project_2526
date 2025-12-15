using MobyPark_api.Dtos.ParkingLot;

namespace MobyPark_api.Services.ParkingLot
{
    public interface IParkingLotService
    {
        Task<IEnumerable<ParkingLotDto>> GetAllAsync();
        Task<ParkingLotDto> GetByIdAsync(long id);
        Task<ParkingLotDto> CreateAsync(ParkingLotCreateDto dto);
        Task<ParkingLotDto> UpdateAsync(long id, ParkingLotUpdateDto dto);
        Task<bool> DeleteAsync(long id);
    }
}
