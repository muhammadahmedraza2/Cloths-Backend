using System.Linq.Expressions;

namespace ClothingErp.Api.Interfaces;

/// <summary>
/// Generic data-access contract. Every EF Core entity (MasterRecord, AppUser,
/// CartItem, Order, ...) is accessed through this same interface, so
/// controllers/services never talk to DbContext directly.
/// </summary>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(object id);
    Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, Func<IQueryable<T>, IQueryable<T>>? include = null);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> filter, Func<IQueryable<T>, IQueryable<T>>? include = null);
    Task AddAsync(T entity);
    void Update(T entity);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
    Task<int> SaveChangesAsync();
}