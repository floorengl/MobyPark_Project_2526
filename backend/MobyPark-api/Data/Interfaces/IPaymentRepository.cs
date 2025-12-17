public interface IPaymentRepository : IGenericRepository<Payment, Guid>
{
    Task<Payment?> GetByIdPaymentAsync(Guid id, CancellationToken ct = default);
    Task<List<Payment>> GetBetweenAsync(DateTime start, DateTime end, CancellationToken ct = default);
    Task<List<Payment>> GetAllPaymentsAsync(CancellationToken ct = default);
}
