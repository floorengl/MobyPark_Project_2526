using MobyPark_api.Data.Models;

public interface ISessionRepository : IGenericRepository<Session, long>
{
    Task<Session?> GetOpenForPlateAsync(long parkingLotId, long licensePlateId, CancellationToken ct = default);
    Task<string> GetPlateTextAsync(long licensePlateId, CancellationToken ct = default);
    Task<List<Reservation>> GetReservationForPlateAsync(long parkingLotId, string licensePlate, CancellationToken ct = default);
    Task<ParkingLot?> GetParkingLotAsync(long parkingLotId, CancellationToken ct = default);
}
