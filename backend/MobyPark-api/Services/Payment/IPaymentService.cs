public interface IPaymentService
{
    Task AddPayment(AddPaymentDto dto);
    Task<List<Payment>> GetPaymentsBetweenAsync(DateTime start, DateTime end);
}