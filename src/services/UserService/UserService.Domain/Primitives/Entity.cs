namespace UserService.Domain.Primitives;

/// <summary>
/// Base entity class for all domain entities following DDD principles.
/// Provides common properties and equality comparison based on identity.
/// </summary>
/// <typeparam name="TId">The type of the entity identifier</typeparam>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
  /// <summary>
  /// Gets the unique identifier for the entity.
  /// </summary>
  public TId Id { get; protected set; }

  /// <summary>
  /// Gets the date and time when the entity was created (UTC).
  /// </summary>
  public DateTime CreatedAt { get; protected set; }

  /// <summary>
  /// Gets the date and time when the entity was last updated (UTC).
  /// </summary>
  public DateTime? UpdatedAt { get; protected set; }

  /// <summary>
  /// Initializes a new instance of the Entity class.
  /// </summary>
  /// <param name="id">The entity identifier</param>
  protected Entity(TId id)
  {
    Id = id;
    CreatedAt = DateTime.UtcNow;
  }

  /// <summary>
  /// Parameterless constructor for EF Core.
  /// </summary>
  protected Entity()
  {
    Id = default!;
    CreatedAt = DateTime.UtcNow;
  }

  /// <summary>
  /// Marks the entity as updated with the current UTC timestamp.
  /// </summary>
  protected void MarkAsUpdated()
  {
    UpdatedAt = DateTime.UtcNow;
  }

  #region Equality Members

  public bool Equals(Entity<TId>? other)
  {
    if (other is null) return false;
    if (ReferenceEquals(this, other)) return true;
    return EqualityComparer<TId>.Default.Equals(Id, other.Id);
  }

  public override bool Equals(object? obj)
  {
    if (obj is null) return false;
    if (ReferenceEquals(this, obj)) return true;
    if (obj.GetType() != GetType()) return false;
    return Equals((Entity<TId>)obj);
  }

  public override int GetHashCode()
  {
    return EqualityComparer<TId>.Default.GetHashCode(Id);
  }

  public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
  {
    return Equals(left, right);
  }

  public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
  {
    return !Equals(left, right);
  }

  #endregion
}
