using Microsoft.EntityFrameworkCore;

public sealed class PaymentService : IPaymentService
{
    private readonly AppDbContext _context;

    public PaymentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Payment>> GetPaymentsBetweenAsync(DateTime start, DateTime end)
    {
        var result = await _context.Payments
            .Where(p => p.CreatedAt >= start && p.CreatedAt <= end)
            .ToListAsync();

        return result;
    }
}