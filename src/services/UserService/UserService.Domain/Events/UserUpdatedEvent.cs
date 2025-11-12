using UserService.Domain.Primitives;

namespace UserService.Domain.Events;

/// <summary>
/// Domain event raised when a user's profile is updated.
/// </summary>
public sealed class UserUpdatedEvent : IDomainEvent
{
  /// <summary>
  /// Initializes a new instance of the UserUpdatedEvent class.
  /// </summary>
  /// <param name="userId">The user identifier</param>
  /// <param name="email">The user email</param>
  /// <param name="fullName">The updated full name</param>
  public UserUpdatedEvent(Guid userId, string email, string fullName)
  {
    EventId = Guid.NewGuid();
    OccurredAt = DateTime.UtcNow;
    UserId = userId;
    Email = email;
    FullName = fullName;
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
  /// Gets the updated full name.
  /// </summary>
  public string FullName { get; }
}
