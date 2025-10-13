using Microsoft.EntityFrameworkCore;

public sealed class PaymentService : IPaymentService
{
    private readonly AppDbContext _db;

    public PaymentService(AppDbContext db)
    {
        _db = db;
    }


    public async Task AddPayment(AddPaymentDto dto)
    {
        var payment = new Payment { Id = dto.Id, CreatedAt = dto.CreatedAt, Status = dto.Status, Hash = dto.Hash, TData = dto.TData };
        _db.Payments.Add(payment);
    }

    public async Task<List<Payment>> GetPaymentsBetweenAsync(DateTime start, DateTime end)
    {
        var result = await _db.Payments
            .Where(p => p.CreatedAt >= start && p.CreatedAt <= end)
            .ToListAsync();

        return result;
    }
}