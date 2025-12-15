using Microsoft.EntityFrameworkCore;

public sealed class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _repo;

    public PaymentService(IPaymentRepository repo) => _repo = repo;

    public async Task AddPaymentAsync(AddPaymentDto dto, CancellationToken ct = default)
    {
        var tx = new TransactionData
        {
            TransactionId = Guid.NewGuid(),
            Amount = dto.Transaction.Amount,
            Date = dto.Transaction.Date,
            Method = dto.Transaction.Method,
            Issuer = dto.Transaction.Issuer,
            Bank = dto.Transaction.Bank
        };

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            Amount = dto.Amount,
            CreatedAt = dto.CreatedAt,
            Status = dto.Status,
            Hash = string.IsNullOrWhiteSpace(dto.Hash) ? Guid.NewGuid().ToString("N") : dto.Hash!,
            TransactionId = tx.TransactionId,
            TransactionData = tx
        };

        await _repo.AddAsync(payment, ct);
        await _repo.SaveChangesAsync(ct);
    }

    public Task<List<Payment>> GetPaymentsBetweenAsync(DateTime start, DateTime end, CancellationToken ct = default)
        => _repo.GetBetweenAsync(start, end, ct);

    public async Task AddRefundAsync(RefundPaymentDto dto, CancellationToken ct = default)
    {
        var original = await _repo.GetByIdWithTransactionAsync(dto.OriginalPaymentId, ct);
        if (original == null)
            throw new InvalidOperationException("Original payment not found.");

        var now = DateTime.UtcNow;

        var refundTx = new TransactionData
        {
            TransactionId = Guid.NewGuid(),
            Amount = -Math.Abs(original.TransactionData.Amount),
            Date = now,
            Method = original.TransactionData.Method,
            Issuer = original.TransactionData.Issuer,
            Bank = original.TransactionData.Bank
        };

        var refund = new Payment
        {
            Id = Guid.NewGuid(),
            Amount = -Math.Abs(original.Amount),
            CreatedAt = now,
            Status = dto.Status,
            Hash = Guid.NewGuid().ToString("N"),
            TransactionId = refundTx.TransactionId,
            TransactionData = refundTx
        };

        await _repo.AddAsync(refund, ct);
        await _repo.SaveChangesAsync(ct);
    }

    public Task<Payment?> GetPaymentByIdAsync(Guid id, CancellationToken ct = default)
        => _repo.GetByIdWithTransactionAsync(id, ct);

    public async Task<Payment?> UpdatePaymentAsync(Guid id, UpdatePaymentDto dto, CancellationToken ct = default)
    {
        var payment = await _repo.GetByIdWithTransactionAsync(id, ct);
        if (payment == null) return null;

        payment.Status = dto.Status;
        payment.TransactionData.Amount = dto.Transaction.Amount;
        payment.TransactionData.Date = dto.Transaction.Date;
        payment.TransactionData.Method = dto.Transaction.Method;
        payment.TransactionData.Issuer = dto.Transaction.Issuer;
        payment.TransactionData.Bank = dto.Transaction.Bank;

        await _repo.SaveChangesAsync(ct);
        return payment;
    }

    public Task<List<Payment>> GetPaymentsAsync(CancellationToken ct = default)
        => _repo.GetAllWithTransactionAsync(ct);
}
