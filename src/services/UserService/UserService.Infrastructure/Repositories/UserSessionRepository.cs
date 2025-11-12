using Microsoft.EntityFrameworkCore;
using UserService.Application.Common.Interfaces.Persistence;
using UserService.Domain.Entities;
using UserService.Infrastructure.Persistence;

namespace UserService.Infrastructure.Repositories;

/// <summary>
/// Repository for UserSession entity operations.
/// </summary>
public sealed class UserSessionRepository : Repository<UserSession, Guid>, IUserSessionRepository
{
  public UserSessionRepository(ApplicationDbContext context) : base(context)
  {
  }

  public async Task<UserSession?> GetByRefreshTokenAsync(
      string refreshToken,
      CancellationToken cancellationToken = default)
  {
    return await Context.UserSessions
        .FirstOrDefaultAsync(s => s.RefreshToken == refreshToken, cancellationToken);
  }

  public async Task<IReadOnlyList<UserSession>> GetActiveSessionsByUserIdAsync(
      Guid userId,
      CancellationToken cancellationToken = default)
  {
    return await Context.UserSessions
        .Where(s => s.UserId == userId && s.IsActive && s.ExpiresAt > DateTime.UtcNow)
        .OrderByDescending(s => s.LastUsedAt)
        .ToListAsync(cancellationToken);
  }

  public async Task RevokeAllUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default)
  {
    var sessions = await Context.UserSessions
        .Where(s => s.UserId == userId && s.IsActive)
        .ToListAsync(cancellationToken);

    foreach (var session in sessions)
    {
      session.Revoke();
    }
  }

  public async Task<int> RemoveExpiredSessionsAsync(CancellationToken cancellationToken = default)
  {
    var expiredSessions = await Context.UserSessions
        .Where(s => s.ExpiresAt <= DateTime.UtcNow)
        .ToListAsync(cancellationToken);

    Context.UserSessions.RemoveRange(expiredSessions);

    return expiredSessions.Count;
  }
}
