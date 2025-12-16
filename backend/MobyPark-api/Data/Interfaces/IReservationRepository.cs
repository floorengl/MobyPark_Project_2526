using MobyPark_api.Data.Models;

public interface IReservationRepository : IGenericRepository<Reservation, Guid>
{
    Task<List<Reservation>> GetAllNoTrackingAsync(CancellationToken ct = default);
    Task<Reservation?> GetByIdNoTrackingAsync(Guid id, CancellationToken ct = default);
    Task<Reservation?> GetActiveReservationEntityAsync(string licensePlate, DateTime time, CancellationToken ct = default);
    Task<(DateTime start, DateTime end)[]> GetOverlappingUnusedIntervalsAsync(
        long parkingLotId, DateTime start, DateTime end, CancellationToken ct = default);
}
