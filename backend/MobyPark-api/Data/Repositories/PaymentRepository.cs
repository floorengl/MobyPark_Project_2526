using Microsoft.EntityFrameworkCore;

public sealed class PaymentRepository : GenericRepository<Payment, Guid>, IPaymentRepository
{
    public PaymentRepository(AppDbContext db) : base(db) { }

    public Task<Payment?> GetByIdPaymentAsync(Guid id, CancellationToken ct = default)
        => _db.Payments
            .Include(p => p.TransactionData)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<List<Payment>> GetBetweenAsync(DateTime start, DateTime end, CancellationToken ct = default)
        => _db.Payments
            .Include(p => p.TransactionData)
            .Where(p => p.CreatedAt >= start && p.CreatedAt <= end)
            .OrderByDescending(p => p.CreatedAt)
            .AsNoTracking()
            .ToListAsync(ct);

    public Task<List<Payment>> GetAllPaymentsAsync(CancellationToken ct = default)
        => _db.Payments
            .Include(p => p.TransactionData)
            .OrderByDescending(p => p.CreatedAt)
            .AsNoTracking()
            .ToListAsync(ct);
}
