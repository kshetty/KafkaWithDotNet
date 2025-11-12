namespace UserService.Domain.Primitives;

/// <summary>
/// Base class for aggregate roots in DDD.
/// Aggregate roots are entities that control access to other entities in the aggregate.
/// They also manage domain events.
/// </summary>
/// <typeparam name="TId">The type of the entity identifier</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
  private readonly List<IDomainEvent> _domainEvents = new();

  /// <summary>
  /// Gets the collection of domain events raised by this aggregate.
  /// </summary>
  public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

  /// <summary>
  /// Initializes a new instance of the AggregateRoot class.
  /// </summary>
  /// <param name="id">The entity identifier</param>
  protected AggregateRoot(TId id) : base(id)
  {
  }

  /// <summary>
  /// Parameterless constructor for EF Core.
  /// </summary>
  protected AggregateRoot() : base()
  {
  }

  /// <summary>
  /// Raises a domain event to be dispatched later.
  /// </summary>
  /// <param name="domainEvent">The domain event to raise</param>
  protected void RaiseDomainEvent(IDomainEvent domainEvent)
  {
    _domainEvents.Add(domainEvent);
  }

  /// <summary>
  /// Clears all domain events. This should be called after events are dispatched.
  /// </summary>
  public void ClearDomainEvents()
  {
    _domainEvents.Clear();
  }
}
