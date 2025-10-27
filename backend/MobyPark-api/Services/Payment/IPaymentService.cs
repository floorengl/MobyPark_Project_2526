public interface IPaymentService
{
    Task AddPaymentAsync(AddPaymentDto dto);
    Task<List<Payment>> GetPaymentsBetweenAsync(DateTime start, DateTime end);
    Task AddRefundAsync(RefundPaymentDto dto);
    Task<Payment?> GetPaymentByIdAsync(long id);
    Task<Payment?> UpdatePaymentAsync(long id, UpdatePaymentDto dto);
    Task<List<Payment>> GetPaymentsAsync();
}