using System.Linq.Expressions;
using EduManager.Data;
using Microsoft.EntityFrameworkCore;

namespace EduManager.Repository;

public class GenericRepository<TEntity> : IRepository<TEntity>
    where TEntity : class
{
    private EduDbContext _context;
    private DbSet<TEntity> _dbset;
    public GenericRepository(EduDbContext context)
    {
        _context = context;
        _dbset = _context.Set<TEntity>();
    }
    public void Add(TEntity entity)
    {
        _dbset.Add(entity);
    }

    public void Delete(TEntity entity)
    {
        _dbset.Remove(entity);
    }

    public async Task<TEntity?> FindByIdAsync(object id)
    {
        return await _dbset.FindAsync(id);
    }

    public IEnumerable<TEntity> GetAll()
    {
        return _dbset;
    }

    public async Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>>? filter = null, string[]? include = null)
    {
        IQueryable<TEntity> query = _dbset;
        if (include != null)
        {
            foreach (string value in include)
            {
                query = query.Include(value);
            }
        }
        if (filter != null)
        {
            query = query.Where(filter);
        }
        return await query.ToListAsync();
    }

    public async Task<List<TResult>> GetAllAsync<TResult>(Expression<Func<TEntity, TResult>> transform, string[]? include = null)
    {
        IQueryable<TEntity> query = _dbset;
        if (include != null)
        {
            foreach (string value in include)
            {
                query = query.Include(value);
            }
        }
        return await query.Select(transform).ToListAsync();
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }

    public void Update(TEntity entity)
    {
        _dbset.Update(entity);
    }
}