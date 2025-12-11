public interface ISessionService
{
    Task<long> StartForPlateAsync(long parkingLotId, long licensePlateId, CancellationToken ct);
    Task StopOpenForPlateAsync(long parkingLotId, long licensePlateId, CancellationToken ct);
}
