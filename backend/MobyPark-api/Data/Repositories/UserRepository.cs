using Microsoft.EntityFrameworkCore;

public sealed class UserRepository : GenericRepository<User, long>, IUserRepository
{
    public UserRepository(AppDbContext db) : base(db) { }

    public Task<bool> UsernameExistsAsync(string username, CancellationToken ct)
        => _db.Users.AnyAsync(u => u.Username == username, ct);

    public Task<User?> GetByUsernameAsync(string username, CancellationToken ct)
        => _db.Users.FirstOrDefaultAsync(u => u.Username == username, ct);

    public Task<User?> GetByIdNoTrackingAsync(long id, CancellationToken ct)
        => _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct);
}
