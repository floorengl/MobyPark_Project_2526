public sealed class PaymentService : IPaymentService
{
    private List<Payment> _payments = new List<Payment>();
    public async Task<List<Payment>> GetPaymentsBetweenAsync(DateTime start, DateTime end)
    {
        await Task.Delay(500);

        var result = _payments.Where(p => p.CreatedAt >= start && p.CreatedAt <= end).ToList();

        return result;
    }
}