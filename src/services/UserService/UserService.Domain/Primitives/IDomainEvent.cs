namespace UserService.Domain.Primitives;

/// <summary>
/// Marker interface for domain events.
/// Domain events represent something that happened in the domain that domain experts care about.
/// </summary>
public interface IDomainEvent
{
  /// <summary>
  /// Gets the unique identifier of the domain event.
  /// </summary>
  Guid EventId { get; }

  /// <summary>
  /// Gets the date and time when the event occurred (UTC).
  /// </summary>
  DateTime OccurredAt { get; }
}
