public interface IPaymentService
{
    Task<List<Payment>> GetPaymentsBetweenAsync(DateTime start, DateTime end);

}