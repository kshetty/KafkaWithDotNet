using KafkaFlow;
using KafkaFlow.Producers;
using UserService.Application.Common.Interfaces.Services;
using UserService.Domain.Primitives;

namespace UserService.Infrastructure.Kafka;

/// <summary>
/// Kafka-based event publisher for domain events.
/// </summary>
public sealed class KafkaEventPublisher : IEventPublisher
{
  private readonly IProducerAccessor _producerAccessor;

  public KafkaEventPublisher(IProducerAccessor producerAccessor)
  {
    _producerAccessor = producerAccessor;
  }

  public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
      where TEvent : IDomainEvent
  {
    var producer = _producerAccessor.GetProducer("user-service-producer");

    var topic = GetTopicName(typeof(TEvent));

    await producer.ProduceAsync(
        topic,
        @event.EventId.ToString(), // Use event ID as message key for partitioning
        @event);
  }

  public async Task PublishManyAsync(
      IEnumerable<IDomainEvent> events,
      CancellationToken cancellationToken = default)
  {
    var producer = _producerAccessor.GetProducer("user-service-producer");

    var tasks = events.Select(e =>
    {
      var topic = GetTopicName(e.GetType());
      return producer.ProduceAsync(topic, e.EventId.ToString(), e);
    });

    await Task.WhenAll(tasks);
  }

  private static string GetTopicName(Type eventType)
  {
    // Convert event type name to kebab-case topic name
    // e.g., UserRegisteredEvent -> user-registered
    var eventName = eventType.Name;

    if (eventName.EndsWith("Event"))
    {
      eventName = eventName[..^5]; // Remove "Event" suffix
    }

    // Convert PascalCase to kebab-case
    return string.Concat(
        eventName.Select((c, i) =>
            i > 0 && char.IsUpper(c) ? "-" + char.ToLower(c) : char.ToLower(c).ToString()));
  }
}
