using System.Linq.Expressions;

public interface IGenericRepository<TEntity, TKey>
    where TEntity : class
{
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken ct = default);
    Task<List<TEntity>> GetAllAsync(CancellationToken ct = default);

    Task AddAsync(TEntity entity, CancellationToken ct = default);
    void Update(TEntity entity);
    void Remove(TEntity entity);

    //IQueryable<TEntity> Query(); // for advanced queries
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
