using UserService.Domain.Primitives;

namespace UserService.Domain.Events;

/// <summary>
/// Domain event raised when a user signs out.
/// </summary>
public sealed class UserSignedOutEvent : IDomainEvent
{
  /// <summary>
  /// Initializes a new instance of the UserSignedOutEvent class.
  /// </summary>
  /// <param name="userId">The user identifier</param>
  /// <param name="email">The user email</param>
  /// <param name="sessionId">The session identifier</param>
  public UserSignedOutEvent(Guid userId, string email, Guid sessionId)
  {
    EventId = Guid.NewGuid();
    OccurredAt = DateTime.UtcNow;
    UserId = userId;
    Email = email;
    SessionId = sessionId;
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
}
