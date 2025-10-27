using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using MobyPark_api.Enums;

public sealed class PaymentService : IPaymentService
{
    private readonly AppDbContext _db;

    public PaymentService(AppDbContext db)
    {
        _db = db;
    }

    //Create a payment
    public async Task AddPaymentAsync(AddPaymentDto dto)
    {
        var payment = new Payment
        {
            Id = dto.Id,
            CreatedAt = dto.CreatedAt,
            Status = dto.Status,
            Hash = dto.Hash,
            TData = dto.TData
        };
        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();
    }

    //Get payment between 2 dates
    public async Task<List<Payment>> GetPaymentsBetweenAsync(DateTime start, DateTime end)
    {
        return await _db.Payments
            .Where(p => p.CreatedAt >= start && p.CreatedAt <= end)
            .ToListAsync();
    }

    //POST refund
    public async Task AddRefundAsync(RefundPaymentDto dto)
    {
        var tData = JsonSerializer.Deserialize<Dictionary<string, object>>(dto.TData);
        var amount = Convert.ToDecimal(tData["amount"]);
        amount = -Math.Abs(amount);

        var payment = new Payment
        {
            CreatedAt = DateTime.UtcNow,
            Status = dto.Status,
            Hash = Guid.NewGuid().ToString("N"),
            TData = dto.TData
        };

        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();
    }

    //Get payment by ID
    public async Task<Payment?> GetPaymentByIdAsync(long id)
    {
        return await _db.Payments.FirstOrDefaultAsync(p => p.Id == id);
    }

    //Update payent by ID
    public async Task<Payment?> UpdatePaymentAsync(long id, UpdatePaymentDto dto)
    {
        var payment = await _db.Payments.FirstOrDefaultAsync(p => p.Id == id);
        if (payment == null) return null;

        var tData = JsonSerializer.Deserialize<Dictionary<string, object>>(dto.TData);
        var amount = Convert.ToDecimal(tData["amount"]);

        payment.TData = dto.TData;
        payment.Status = dto.Status;

        await _db.SaveChangesAsync();
        return payment;
    }

    // Get all payments
    public async Task<List<Payment>> GetPaymentsAsync()
    {
        return await _db.Payments
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }
}