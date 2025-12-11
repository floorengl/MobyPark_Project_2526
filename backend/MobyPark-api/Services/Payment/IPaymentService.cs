public interface IPaymentService
{
    Task AddPaymentAsync(AddPaymentDto dto);
    Task<List<Payment>> GetPaymentsBetweenAsync(DateTime start, DateTime end);
    Task AddRefundAsync(RefundPaymentDto dto);
    Task<Payment?> GetPaymentByIdAsync(Guid id);
    Task<Payment?> UpdatePaymentAsync(Guid id, UpdatePaymentDto dto);
    Task<List<Payment>> GetPaymentsAsync();
}