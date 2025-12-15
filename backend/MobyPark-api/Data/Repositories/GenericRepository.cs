using Microsoft.EntityFrameworkCore;

public abstract class GenericRepository<TEntity, TKey> : IGenericRepository<TEntity, TKey>
    where TEntity : class
{
    protected readonly AppDbContext _db;
    protected readonly DbSet<TEntity> _set;

    protected GenericRepository(AppDbContext db)
    {
        _db = db;
        _set = db.Set<TEntity>();
    }

    public virtual async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken ct = default)
        => await _set.FindAsync([id], ct);

    public virtual async Task<List<TEntity>> GetAllAsync(CancellationToken ct = default)
        => await _set.AsNoTracking().ToListAsync(ct);

    public virtual async Task AddAsync(TEntity entity, CancellationToken ct = default)
        => await _set.AddAsync(entity, ct);

    public virtual void Update(TEntity entity) => _set.Update(entity);

    public virtual void Remove(TEntity entity) => _set.Remove(entity);

    public virtual IQueryable<TEntity> Query() => _set.AsQueryable();

    public virtual Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);

    // Optional EF-style “TestConnection”
    public virtual async Task<bool> TestConnectionAsync(CancellationToken ct = default)
    {
        try { return await _db.Database.CanConnectAsync(ct); }
        catch { return false; }
    }
}
