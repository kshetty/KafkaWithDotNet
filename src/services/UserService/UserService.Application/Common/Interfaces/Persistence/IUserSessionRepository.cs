using UserService.Domain.Entities;

namespace UserService.Application.Common.Interfaces.Persistence;

/// <summary>
/// Repository interface for UserSession entity operations.
/// </summary>
public interface IUserSessionRepository : IRepository<UserSession, Guid>
{
  /// <summary>
  /// Gets a session by its refresh token.
  /// </summary>
  /// <param name="refreshToken">The refresh token</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>The session if found, null otherwise</returns>
  Task<UserSession?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

  /// <summary>
  /// Gets all active sessions for a user.
  /// </summary>
  /// <param name="userId">The user identifier</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>Collection of active sessions</returns>
  Task<IReadOnlyList<UserSession>> GetActiveSessionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

  /// <summary>
  /// Revokes all sessions for a user.
  /// </summary>
  /// <param name="userId">The user identifier</param>
  /// <param name="cancellationToken">Cancellation token</param>
  Task RevokeAllUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default);

  /// <summary>
  /// Removes expired sessions (cleanup operation).
  /// </summary>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>Number of sessions removed</returns>
  Task<int> RemoveExpiredSessionsAsync(CancellationToken cancellationToken = default);
}
