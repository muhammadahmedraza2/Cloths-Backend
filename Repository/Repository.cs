using System.Linq.Expressions;
using ClothingErp.Api.Data;
using ClothingErp.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClothingErp.Api.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly AppDbContext _db;
    private readonly DbSet<T> _set;

    public Repository(AppDbContext db)
    {
        _db = db;
        _set = db.Set<T>();
    }

    public async Task<T?> GetByIdAsync(object id) => await _set.FindAsync(id);

    public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, Func<IQueryable<T>, IQueryable<T>>? include = null)
    {
        IQueryable<T> query = _set.AsNoTracking();
        if (include is not null) query = include(query);
        if (filter is not null) query = query.Where(filter);
        return await query.ToListAsync();
    }

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> filter, Func<IQueryable<T>, IQueryable<T>>? include = null)
    {
        IQueryable<T> query = _set;
        if (include is not null) query = include(query);
        return await query.FirstOrDefaultAsync(filter);
    }

    public async Task AddAsync(T entity) => await _set.AddAsync(entity);

    public void Update(T entity) => _set.Update(entity);

    public void Remove(T entity) => _set.Remove(entity);

    public void RemoveRange(IEnumerable<T> entities) => _set.RemoveRange(entities);

    public async Task<int> SaveChangesAsync() => await _db.SaveChangesAsync();
}