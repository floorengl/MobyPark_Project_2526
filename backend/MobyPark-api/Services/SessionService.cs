using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MobyPark_api.Enums;

public sealed class SessionService : ISessionService
{
    private readonly AppDbContext _db;
    private readonly IPaymentService _paymentService;
    public SessionService(AppDbContext db, IPaymentService paymentService)
    {
        _db = db;
        _paymentService = paymentService;
    }

    // Start session
    public async Task<long> StartForPlateAsync(long parkingLotId, long licensePlateId, CancellationToken ct)
    {
        var open = await _db.Sessions
            .FirstOrDefaultAsync(s => s.ParkingLotId == parkingLotId
                                && s.LicensePlateId == licensePlateId
                                && s.Stopped == null, ct);
        if (open != null) return open.Id;

        var plateText = await _db.LicensePlates
            .Where(p => p.Id == licensePlateId)
            .Select(p => p.LicensePlateName)
            .FirstAsync(ct);

        var session = new Session
        {
            ParkingLotId = parkingLotId,
            LicensePlateId = licensePlateId,
            PlateText = plateText!,
            Started = DateTime.UtcNow
        };

        _db.Sessions.Add(session);
        await _db.SaveChangesAsync(ct);
        return session.Id;
    }

    // Stop session
    public async Task StopOpenForPlateAsync(long parkingLotId, long licensePlateId, CancellationToken ct)
    {
        var session = await _db.Sessions
            .FirstOrDefaultAsync(s => s.ParkingLotId == parkingLotId
                                   && s.LicensePlateId == licensePlateId
                                   && s.Stopped == null, ct);
        if (session == null) return;
        // Total minutes of the session.
        session.Stopped = DateTime.UtcNow;
        var minutes = Math.Max(1, (int)Math.Ceiling((session.Stopped.Value - session.Started).TotalMinutes));
        session.DurationMinutes = (short)Math.Min(short.MaxValue, minutes);
        // Saves the current changes to the databases
        await _db.SaveChangesAsync(ct);
        // Get the parking lot.
        var parkinglot = await _db.ParkingLots
            .FirstOrDefaultAsync(p => p.Id == parkingLotId);
        // Get the tariff for the parking lot.
        var pricePerHour = parkinglot?.Tariff ?? 0;
        // Calculate the tarfiff for the session in the parking lot.
        session.Cost = (float)(minutes / 60.0) * pricePerHour;

        var tData = new TransactionData
        {
            Amount = (decimal)session.Cost,
            Date = DateTime.UtcNow,
            Method = "ideal",
            Issuer = "XYY910HH",
            Bank = "ABN-NL"
        };

        var payment = new Payment
        {
            Amount = (decimal)session.Cost,
            CreatedAt = DateTime.UtcNow,
            Status = PaymentStatus.Complete,
            TransactionData = tData
        };

        _db.Payments.Add(payment);
        await _db.SaveChangesAsync(ct);
    }
}
