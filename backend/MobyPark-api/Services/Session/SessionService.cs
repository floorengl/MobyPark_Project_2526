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

        session.Stopped = DateTime.UtcNow;
        var minutes = Math.Max(1, (int)Math.Ceiling((session.Stopped.Value - session.Started).TotalMinutes));
        session.DurationMinutes = (short)Math.Min(short.MaxValue, minutes);
        session.Cost = null;
        session.PlateText = "";

        await _db.SaveChangesAsync(ct);


        var parkinglot = await _db.ParkingLots
            .FirstOrDefaultAsync(p => p.Id == parkingLotId);

        var pricePerHour = parkinglot?.Tariff ?? 0;
        session.Cost = (float)(minutes / 60.0) * pricePerHour;

        var tData = new
        {
            amount = session.Cost,
            date = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
            method = "ideal",
            issuer = "XYY910HH",
            bank = "ABN-NL"
        };

        var paymentDto = new AddPaymentDto
        {
            Id = DateTime.UtcNow.Ticks,
            CreatedAt = DateTime.UtcNow,
            Status = PaymentStatus.Complete,
            Hash = "PlaceholderHash",
            TData = JsonSerializer.Serialize(tData)
        };

        await _paymentService.AddPaymentAsync(paymentDto);
    }
}
