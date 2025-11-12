using UserService.Domain.Primitives;

namespace UserService.Domain.Events;

/// <summary>
/// Domain event raised when a user successfully signs in.
/// </summary>
public sealed class UserSignedInEvent : IDomainEvent
{
  /// <summary>
  /// Initializes a new instance of the UserSignedInEvent class.
  /// </summary>
  /// <param name="userId">The user identifier</param>
  /// <param name="email">The user email</param>
  /// <param name="sessionId">The session identifier</param>
  /// <param name="ipAddress">The IP address of the sign-in</param>
  /// <param name="userAgent">The user agent of the sign-in</param>
  public UserSignedInEvent(Guid userId, string email, Guid sessionId, string? ipAddress, string? userAgent)
  {
    EventId = Guid.NewGuid();
    OccurredAt = DateTime.UtcNow;
    UserId = userId;
    Email = email;
    SessionId = sessionId;
    IpAddress = ipAddress;
    UserAgent = userAgent;
  }

  /// <inheritdoc />
  public Guid EventId { get; }

  /// <inheritdoc />
  public DateTime OccurredAt { get; }

  /// <summary>
  /// Gets the user identifier.
  /// </summary>
  public Guid UserId { get; }

  /// <summary>
  /// Gets the user email.
  /// </summary>
  public string Email { get; }

  /// <summary>
  /// Gets the session identifier.
  /// </summary>
  public Guid SessionId { get; }

  /// <summary>
  /// Gets the IP address of the sign-in.
  /// </summary>
  public string? IpAddress { get; }

  /// <summary>
  /// Gets the user agent of the sign-in.
  /// </summary>
  public string? UserAgent { get; }
}
