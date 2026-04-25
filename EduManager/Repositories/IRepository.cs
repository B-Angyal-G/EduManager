using System.Linq.Expressions;

namespace EduManager.Repository;

public interface IRepository<TEntity>
{
    Task<TEntity?> FindByIdAsync(object id);
    IEnumerable<TEntity> GetAll();
    Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>>? filter = null, string[]? include = null);
    Task<List<TResult>> GetAllAsync<TResult>(Expression<Func<TEntity, TResult>> transform, string[]? include = null);
    void Add(TEntity entity);
    void Update(TEntity entity);
    void Delete(TEntity entity);
    Task SaveAsync();
}