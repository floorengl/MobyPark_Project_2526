using MobyPark_api.Data.Models;
using MobyPark_api.Enums;

public sealed class SessionService : ISessionService
{
    private readonly ISessionRepository _repo;
    private readonly IPaymentService _paymentService;
    private readonly IPricingService _pricingService;

    public SessionService(ISessionRepository repo, IPaymentService paymentService, IPricingService pricingService)
        => (_repo, _paymentService, _pricingService) = (repo, paymentService, pricingService);

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
        // get open sessions for plate
        var session = await _repo.GetOpenForPlateAsync(parkingLotId, licensePlateId, ct);
        if (session == null) return;

        session.Stopped = DateTime.UtcNow;

        // check for reservations
        string plateText = await _repo.GetPlateTextAsync(licensePlateId, ct);
        List<Reservation> reservationsByPlate =
            await _repo.GetReservationForPlateAsync(parkingLotId, plateText, ct);

        // total session minutes
        var totalMinutes = Math.Max(1,
            (int)Math.Ceiling((session.Stopped.Value - session.Started).TotalMinutes));
        session.DurationMinutes = (short)Math.Min(short.MaxValue, totalMinutes);

        decimal sessionCost = await _pricingService.GetPrice(session);

        if (reservationsByPlate == null || reservationsByPlate.Count == 0)
        {
            // pay full session
            session.Cost = sessionCost;
            await _repo.SaveChangesAsync(ct);

            if (session.Cost == 0) return; // session can be free if it is less than 3 minutes

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
            return;
        }

        var chargedReservations = reservationsByPlate
            .Where(r => r.Status != ReservationStatus.UnUsed)
            .ToList();

        // mark reservation as used only after it expires AND was used at least once
        var now = DateTime.UtcNow;
        decimal payedThroughReservation = 0;
        foreach (var reservation in chargedReservations)
        {
            bool wasUsed =
                session.Started < reservation.EndTime &&
                session.Stopped.Value > reservation.StartTime;

            if (wasUsed && now > reservation.EndTime)
            {
                reservation.Status = ReservationStatus.Used;
                payedThroughReservation += reservation.Cost ?? 0;
            }
        }

        sessionCost -= payedThroughReservation;
        if (sessionCost < 0)
            sessionCost = 0;

        session.Cost = sessionCost;

        if (sessionCost == 0)
        {
            await _repo.SaveChangesAsync(ct);
            return;
        }

        // pay overtime
        var parkingLot = await _repo.GetParkingLotAsync(parkingLotId, ct);
        var tariff = parkingLot?.Tariff ?? 0;

        await _repo.SaveChangesAsync(ct);

        var paymentDto = new AddPaymentDto
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

        await _paymentService.AddPaymentAsync(paymentDto, ct);
    }
}
