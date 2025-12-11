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
        // Create TransactionData entity from dto
        var transaction = new TransactionData
        {
            Amount = dto.Transaction.Amount,
            Date = dto.Transaction.Date,
            Method = dto.Transaction.Method,
            Issuer = dto.Transaction.Issuer,
            Bank = dto.Transaction.Bank
        };

        var payment = new Payment
        {
            Amount = dto.Amount,
            CreatedAt = dto.CreatedAt,
            Status = dto.Status,
            Hash = string.IsNullOrWhiteSpace(dto.Hash)
                ? Guid.NewGuid().ToString("N")
                : dto.Hash,
            TransactionData = transaction
        };

        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();
    }

    //Get payment between 2 dates
    public async Task<List<Payment>> GetPaymentsBetweenAsync(DateTime start, DateTime end)
    {
        return await _db.Payments
            .Include(p => p.TransactionData)
            .Where(p => p.CreatedAt >= start && p.CreatedAt <= end)
            .ToListAsync();
    }

    //POST refund
    public async Task AddRefundAsync(RefundPaymentDto dto)
    {
        var original = await _db.Payments
            .Include(p => p.TransactionData)
            .FirstOrDefaultAsync(p => p.Id == dto.OriginalPaymentId);

        if (original == null)
        {
            throw new InvalidOperationException("Original payment not found.");
        }

        var refundTransaction = new TransactionData
        {
            Amount = -Math.Abs(original.TransactionData.Amount),
            Date = DateTime.UtcNow,
            Method = original.TransactionData.Method,
            Issuer = original.TransactionData.Issuer,
            Bank = original.TransactionData.Bank
        };

        var refundPayment = new Payment
        {
            Amount = -Math.Abs(original.Amount),
            CreatedAt = DateTime.UtcNow,
            Status = dto.Status,
            Hash = Guid.NewGuid().ToString("N"),
            TransactionData = refundTransaction
        };

        _db.Payments.Add(refundPayment);
        await _db.SaveChangesAsync();
    }

    //Get payment by ID
    public async Task<Payment?> GetPaymentByIdAsync(Guid id)
    {
        return await _db.Payments.Include(p => p.TransactionData).FirstOrDefaultAsync(p => p.Id == id);
    }

    //Update payment by ID
    public async Task<Payment?> UpdatePaymentAsync(Guid id, UpdatePaymentDto dto)
    {
        var payment = await _db.Payments
            .Include(p => p.TransactionData)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (payment == null) return null;

        payment.Status = dto.Status;

        payment.TransactionData.Amount = dto.Transaction.Amount;
        payment.TransactionData.Date = dto.Transaction.Date;
        payment.TransactionData.Method = dto.Transaction.Method;
        payment.TransactionData.Issuer = dto.Transaction.Issuer;
        payment.TransactionData.Bank = dto.Transaction.Bank;

        await _db.SaveChangesAsync();
        return payment;
    }

    // Get all payments
    public async Task<List<Payment>> GetPaymentsAsync()
    {
        return await _db.Payments
            .Include(p => p.TransactionData)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }
}