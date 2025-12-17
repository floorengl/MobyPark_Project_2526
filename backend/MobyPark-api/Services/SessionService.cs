using MobyPark_api.Enums;

public sealed class SessionService : ISessionService
{
    private readonly ISessionRepository _repo;
    private readonly IPaymentService _paymentService;

    public SessionService(ISessionRepository repo, IPaymentService paymentService)
        => (_repo, _paymentService) = (repo, paymentService);

    // Start session
    public async Task<long> StartForPlateAsync(long parkingLotId, long licensePlateId, CancellationToken ct)
    {
        var open = await _repo.GetOpenForPlateAsync(parkingLotId, licensePlateId, ct);
        if (open != null) return open.Id;
        var plateText = await _repo.GetPlateTextAsync(licensePlateId, ct);
        var session = new Session
        {
            ParkingLotId = parkingLotId,
            LicensePlateId = licensePlateId,
            PlateText = plateText,
            Started = DateTime.UtcNow
        };
        await _repo.AddAsync(session, ct);
        await _repo.SaveChangesAsync(ct);
        return session.Id;
    }

    // Stop session
    public async Task StopOpenForPlateAsync(long parkingLotId, long licensePlateId, CancellationToken ct)
    {
        var session = await _repo.GetOpenForPlateAsync(parkingLotId, licensePlateId, ct);
        if (session == null) return;
        session.Stopped = DateTime.UtcNow;
        // total minutes
        var minutes = Math.Max(1, (int)Math.Ceiling((session.Stopped.Value - session.Started).TotalMinutes));
        session.DurationMinutes = (short)Math.Min(short.MaxValue, minutes);
        // parking lot tariff
        var parkinglot = await _repo.GetParkingLotAsync(parkingLotId, ct);
        var pricePerHour = parkinglot?.Tariff ?? 0;
        session.Cost = (float)(minutes / 60.0) * pricePerHour;
        await _repo.SaveChangesAsync(ct);
        // Create payment
        var addDto = new AddPaymentDto
        {
            Amount = (decimal)session.Cost,
            CreatedAt = DateTime.UtcNow,
            Status = PaymentStatus.Complete,
            Hash = null,
            Transaction = new TransactionDataDto
            {
                Amount = (decimal)session.Cost,
                Date = DateTime.UtcNow,
                Method = "ideal",
                Issuer = "XYY910HH",
                Bank = "ABN-NL"
            }
        };
        await _paymentService.AddPaymentAsync(addDto, ct);
    }
}
