using UserService.Domain.Primitives;

namespace UserService.Application.Common.Interfaces.Services;

/// <summary>
/// Service interface for publishing domain events to Kafka.
/// </summary>
public interface IEventPublisher
{
  /// <summary>
  /// Publishes a domain event to Kafka.
  /// </summary>
  /// <typeparam name="TEvent">The event type</typeparam>
  /// <param name="event">The domain event to publish</param>
  /// <param name="cancellationToken">Cancellation token</param>
  Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
      where TEvent : IDomainEvent;

  /// <summary>
  /// Publishes multiple domain events to Kafka.
  /// </summary>
  /// <param name="events">The domain events to publish</param>
  /// <param name="cancellationToken">Cancellation token</param>
  Task PublishManyAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default);
}
