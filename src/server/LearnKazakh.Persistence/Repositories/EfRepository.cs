using LearnKazakh.Core.Repositories;
using LearnKazakh.Domain.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace LearnKazakh.Persistence.Repositories;

public class EfRepository<T> : IRepository<T> where T : class, IAggregateRoot, new()
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LearnKazakhContext _context;
    private readonly DbSet<T> _dbSet;

    public EfRepository(IHttpContextAccessor httpContextAccessor, LearnKazakhContext context)
    {
        _httpContextAccessor = httpContextAccessor;
        _context = context;
        _dbSet = _context.Set<T>();
    }

    protected string CurrentUser => _httpContextAccessor.HttpContext.User.Identity?.Name ?? "System";

    public IQueryable<T> GetAll()
    {
        return _dbSet.AsNoTracking();
    }

    public IQueryable<T> GetAll(Expression<Func<T, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return _dbSet.AsNoTracking().Where(predicate);
    }

    public IQueryable<T> GetAll(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        IQueryable<T> query = _dbSet.AsNoTracking();

        if (include != null)
        {
            query = include(query);
        }

        return query.Where(predicate);
    }

    public async Task<T?> GetAsync(Expression<Func<T, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return await _dbSet.Where(predicate).FirstOrDefaultAsync();
    }

    public async Task<T?> GetAsync(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null)
    {
        IQueryable<T> query = _dbSet;

        if (include != null)
        {
            query = include(query);
        }

        return await query.Where(predicate).FirstOrDefaultAsync();
    }

    public async Task CreateAsync(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        if (entity is IAuditableEntity auditable)
        {
            auditable.CreatedAt = DateTime.UtcNow;
            auditable.CreatedBy = CurrentUser;
        }

        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        if (entity is IAuditableEntity auditable)
        {
            auditable.LastModifiedAt = DateTime.UtcNow;
            auditable.LastModifiedBy = CurrentUser;
        }

        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        if (entity is ISoftDeletable softDeletable)
        {
            softDeletable.IsDeleted = true;
            softDeletable.DeletedAt = DateTime.UtcNow;
            softDeletable.DeletedBy = CurrentUser;
        }
        else
        {
            _dbSet.Remove(entity);
        }

        await _context.SaveChangesAsync();
    }
}
