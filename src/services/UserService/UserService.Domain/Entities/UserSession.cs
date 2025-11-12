using UserService.Domain.Primitives;

namespace UserService.Domain.Entities;

/// <summary>
/// Represents a user session for managing refresh tokens and active sessions.
/// </summary>
public sealed class UserSession : Entity<Guid>
{
  /// <summary>
  /// Gets the user identifier this session belongs to.
  /// </summary>
  public Guid UserId { get; private set; }

  /// <summary>
  /// Gets the refresh token for this session.
  /// </summary>
  public string RefreshToken { get; private set; }

  /// <summary>
  /// Gets the date and time when the refresh token expires (UTC).
  /// </summary>
  public DateTime ExpiresAt { get; private set; }

  /// <summary>
  /// Gets a value indicating whether this session is still active.
  /// </summary>
  public bool IsActive { get; private set; }

  /// <summary>
  /// Gets the IP address from which the session was created.
  /// </summary>
  public string? IpAddress { get; private set; }

  /// <summary>
  /// Gets the user agent from which the session was created.
  /// </summary>
  public string? UserAgent { get; private set; }

  /// <summary>
  /// Gets the date and time when the session was revoked (if revoked).
  /// </summary>
  public DateTime? RevokedAt { get; private set; }

  /// <summary>
  /// Gets the date and time when the refresh token was last used.
  /// </summary>
  public DateTime? LastUsedAt { get; private set; }

  /// <summary>
  /// Navigation property to the user.
  /// </summary>
  public User User { get; private set; }

  /// <summary>
  /// Private constructor for EF Core.
  /// </summary>
  private UserSession() : base()
  {
    RefreshToken = string.Empty;
    User = null!;
  }

  /// <summary>
  /// Creates a new user session.
  /// </summary>
  /// <param name="userId">The user identifier</param>
  /// <param name="ipAddress">The IP address</param>
  /// <param name="userAgent">The user agent</param>
  /// <param name="expiryDays">Number of days until the refresh token expires</param>
  /// <returns>A new UserSession instance</returns>
  public static UserSession Create(Guid userId, string? ipAddress, string? userAgent, int expiryDays = 7)
  {
    if (expiryDays <= 0)
    {
      throw new ArgumentException("Expiry days must be greater than zero.", nameof(expiryDays));
    }

    var session = new UserSession
    {
      Id = Guid.NewGuid(),
      UserId = userId,
      RefreshToken = GenerateRefreshToken(),
      ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
      IsActive = true,
      IpAddress = ipAddress?.Length > 45 ? ipAddress[..45] : ipAddress, // IPv6 max length
      UserAgent = userAgent?.Length > 500 ? userAgent[..500] : userAgent, // Truncate long user agents
      RevokedAt = null,
      LastUsedAt = null
    };

    return session;
  }

  /// <summary>
  /// Checks if the session is expired.
  /// </summary>
  /// <returns>True if expired, false otherwise</returns>
  public bool IsExpired()
  {
    return DateTime.UtcNow >= ExpiresAt;
  }

  /// <summary>
  /// Checks if the session is valid (active and not expired).
  /// </summary>
  /// <returns>True if valid, false otherwise</returns>
  public bool IsValid()
  {
    return IsActive && !IsExpired();
  }

  /// <summary>
  /// Revokes the session, making it inactive.
  /// </summary>
  public void Revoke()
  {
    if (!IsActive)
    {
      return; // Already revoked
    }

    IsActive = false;
    RevokedAt = DateTime.UtcNow;
    MarkAsUpdated();
  }

  /// <summary>
  /// Marks the session as used and updates the last used timestamp.
  /// </summary>
  public void MarkAsUsed()
  {
    if (!IsValid())
    {
      throw new InvalidOperationException("Cannot use an invalid session.");
    }

    LastUsedAt = DateTime.UtcNow;
    MarkAsUpdated();
  }

  /// <summary>
  /// Refreshes the session by generating a new refresh token and extending expiry.
  /// </summary>
  /// <param name="expiryDays">Number of days until the new refresh token expires</param>
  public void Refresh(int expiryDays = 7)
  {
    if (!IsActive)
    {
      throw new InvalidOperationException("Cannot refresh a revoked session.");
    }

    if (expiryDays <= 0)
    {
      throw new ArgumentException("Expiry days must be greater than zero.", nameof(expiryDays));
    }

    RefreshToken = GenerateRefreshToken();
    ExpiresAt = DateTime.UtcNow.AddDays(expiryDays);
    LastUsedAt = DateTime.UtcNow;
    MarkAsUpdated();
  }

  /// <summary>
  /// Generates a cryptographically secure refresh token.
  /// </summary>
  private static string GenerateRefreshToken()
  {
    var randomBytes = new byte[32];
    using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
    rng.GetBytes(randomBytes);
    return Convert.ToBase64String(randomBytes);
  }
}
