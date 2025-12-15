public interface IPaymentRepository : IGenericRepository<Payment, Guid>
{
    Task<Payment?> GetByIdWithTransactionAsync(Guid id, CancellationToken ct = default);
    Task<List<Payment>> GetBetweenAsync(DateTime start, DateTime end, CancellationToken ct = default);
    Task<List<Payment>> GetAllWithTransactionAsync(CancellationToken ct = default);
}
