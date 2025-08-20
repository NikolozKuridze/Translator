using System.Linq.Expressions;
using Translator.Domain;

namespace Translator.Infrastructure.Database.Postgres.Repository;

public interface IRepository<TEntity>
    where TEntity : BaseEntity
{
    public Task AddAsync(TEntity entity, CancellationToken cancellationToken);
    
    public Task UpdateAsync(TEntity entity);
    
    public Task DeleteAsync(TEntity[] entities);
    
    public IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> predicate);
    
    public IQueryable<TEntity> AsQueryable();
    
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}