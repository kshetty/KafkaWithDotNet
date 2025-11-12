namespace UserService.Application.Common.Interfaces.Persistence;

/// <summary>
/// Base repository interface for common repository operations.
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
/// <typeparam name="TId">The entity identifier type</typeparam>
public interface IRepository<TEntity, TId>
    where TEntity : class
    where TId : notnull
{
  /// <summary>
  /// Gets an entity by its identifier.
  /// </summary>
  /// <param name="id">The entity identifier</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>The entity if found, null otherwise</returns>
  Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

  /// <summary>
  /// Adds a new entity to the repository.
  /// </summary>
  /// <param name="entity">The entity to add</param>
  /// <param name="cancellationToken">Cancellation token</param>
  Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

  /// <summary>
  /// Updates an existing entity.
  /// </summary>
  /// <param name="entity">The entity to update</param>
  void Update(TEntity entity);

  /// <summary>
  /// Removes an entity from the repository.
  /// </summary>
  /// <param name="entity">The entity to remove</param>
  void Remove(TEntity entity);
}
