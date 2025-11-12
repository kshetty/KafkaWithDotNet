using Microsoft.EntityFrameworkCore;
using UserService.Application.Common.Interfaces.Persistence;
using UserService.Domain.Primitives;
using UserService.Infrastructure.Persistence;

namespace UserService.Infrastructure.Repositories;

/// <summary>
/// Base repository implementation with common CRUD operations.
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
/// <typeparam name="TId">The entity identifier type</typeparam>
public abstract class Repository<TEntity, TId> : IRepository<TEntity, TId>
    where TEntity : Entity<TId>
    where TId : notnull
{
  protected readonly ApplicationDbContext Context;

  protected Repository(ApplicationDbContext context)
  {
    Context = context;
  }

  public virtual async Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
  {
    return await Context.Set<TEntity>()
        .FindAsync(new object[] { id }, cancellationToken);
  }

  public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
  {
    await Context.Set<TEntity>().AddAsync(entity, cancellationToken);
  }

  public virtual void Update(TEntity entity)
  {
    Context.Set<TEntity>().Update(entity);
  }

  public virtual void Remove(TEntity entity)
  {
    Context.Set<TEntity>().Remove(entity);
  }
}
