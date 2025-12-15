public interface IPaymentService
{
    Task AddPaymentAsync(AddPaymentDto dto, CancellationToken ct = default);
    Task<List<Payment>> GetPaymentsBetweenAsync(DateTime start, DateTime end, CancellationToken ct = default);
    Task AddRefundAsync(RefundPaymentDto dto, CancellationToken ct = default);
    Task<Payment?> GetPaymentByIdAsync(Guid id, CancellationToken ct = default);
    Task<Payment?> UpdatePaymentAsync(Guid id, UpdatePaymentDto dto, CancellationToken ct = default);
    Task<List<Payment>> GetPaymentsAsync(CancellationToken ct = default);
}
