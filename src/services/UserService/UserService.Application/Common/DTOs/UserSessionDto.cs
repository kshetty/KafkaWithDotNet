namespace UserService.Application.Common.DTOs;

/// <summary>
/// DTO representing a user session.
/// </summary>
public sealed record UserSessionDto
{
  /// <summary>
  /// Gets the session identifier.
  /// </summary>
  public required Guid Id { get; init; }

  /// <summary>
  /// Gets the IP address.
  /// </summary>
  public string? IpAddress { get; init; }

  /// <summary>
  /// Gets the user agent.
  /// </summary>
  public string? UserAgent { get; init; }

  /// <summary>
  /// Gets whether the session is active.
  /// </summary>
  public required bool IsActive { get; init; }

  /// <summary>
  /// Gets the session creation date.
  /// </summary>
  public required DateTime CreatedAt { get; init; }

  /// <summary>
  /// Gets the session expiry date.
  /// </summary>
  public required DateTime ExpiresAt { get; init; }

  /// <summary>
  /// Gets the last used date.
  /// </summary>
  public DateTime? LastUsedAt { get; init; }
}
