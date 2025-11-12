namespace UserService.Application.Common.Interfaces.Persistence;

/// <summary>
/// Unit of Work pattern interface for managing database transactions.
/// Ensures atomic operations across multiple repository operations.
/// </summary>
public interface IUnitOfWork : IDisposable
{
  /// <summary>
  /// Gets the user repository.
  /// </summary>
  IUserRepository Users { get; }

  /// <summary>
  /// Gets the user session repository.
  /// </summary>
  IUserSessionRepository UserSessions { get; }

  /// <summary>
  /// Saves all changes made in this unit of work to the database.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>The number of state entries written to the database</returns>
  Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

  /// <summary>
  /// Begins a new database transaction.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token</param>
  Task BeginTransactionAsync(CancellationToken cancellationToken = default);

  /// <summary>
  /// Commits the current transaction.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token</param>
  Task CommitTransactionAsync(CancellationToken cancellationToken = default);

  /// <summary>
  /// Rolls back the current transaction.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token</param>
  Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
