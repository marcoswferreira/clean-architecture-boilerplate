using Domain.Repositories;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Crosscutting.SqlServer.Repositories;


public abstract class BaseRepository<T> : IBaseRepository<T> where T : class
{
    protected readonly DbContext _dbContext;
    protected readonly DbSet<T> _dbSet;

    public BaseRepository(DbContext context)
    {
        _dbContext = context ?? throw new ArgumentException(nameof(context));
        _dbSet = _dbContext.Set<T>();
    }

    public bool SaveChanges()
    {
        return _dbContext.SaveChanges() > 0;
    }

    public IQueryable<T> Query(Expression<Func<T, bool>>? predicate = null,
                            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
                            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
                            bool enableTracking = true)
    {
        IQueryable<T> query = _dbSet;

        if (!enableTracking) query = query.AsNoTracking();

        if (include != null) query = include(query);

        if (predicate != null) query = query.Where(predicate);

        return orderBy != null ? orderBy(query).AsQueryable() : query.AsQueryable();
    }
    public void Dispose()
    {
        _dbContext?.Dispose();
    }

    public void Delete(T entity)
    {
        _dbSet.Remove(entity);
    }

    public void Delete(IEnumerable<T> entities)
    {
        _dbSet.RemoveRange(entities);
    }

    public T Insert(T entity)
    {
        return _dbSet.Add(entity).Entity;
    }

    public T InsertNotExists(Expression<Func<T, bool>> predicate, T entity)
    {
        if (_dbSet.Any(predicate)) return _dbSet.First(predicate.Compile());
        _dbSet.Add(entity);
        return entity;
    }

    public void Update(T entity)
    {
        _dbContext.Entry(entity).State = EntityState.Modified;
        _dbSet.Update(entity);
    }
}