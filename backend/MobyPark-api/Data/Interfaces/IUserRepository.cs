public interface IUserRepository : IGenericRepository<User, long>
{
    Task<bool> UsernameExistsAsync(string username, CancellationToken ct);
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct);
    Task<User?> GetByIdNoTrackingAsync(long id, CancellationToken ct);
}
