using UserService.Domain.Entities;
using UserService.Domain.ValueObjects;

namespace UserService.Application.Common.Interfaces.Persistence;

/// <summary>
/// Repository interface for User aggregate operations.
/// Combines read and write operations for the User entity.
/// </summary>
public interface IUserRepository : IRepository<User, Guid>
{
  /// <summary>
  /// Gets a user by email address.
  /// </summary>
  /// <param name="email">The email address</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>The user if found, null otherwise</returns>
  Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);

  /// <summary>
  /// Checks if a user with the specified email exists.
  /// </summary>
  /// <param name="email">The email address</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>True if exists, false otherwise</returns>
  Task<bool> ExistsAsync(Email email, CancellationToken cancellationToken = default);

  /// <summary>
  /// Gets a user with their active sessions.
  /// </summary>
  /// <param name="userId">The user identifier</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>The user with sessions if found, null otherwise</returns>
  Task<User?> GetWithSessionsAsync(Guid userId, CancellationToken cancellationToken = default);
}
