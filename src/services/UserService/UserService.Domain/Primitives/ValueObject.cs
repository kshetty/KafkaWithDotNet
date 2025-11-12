namespace UserService.Domain.Primitives;

/// <summary>
/// Base class for value objects in DDD.
/// Value objects are immutable and compared by their values rather than identity.
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
  /// <summary>
  /// Gets the atomic values that define this value object's equality.
  /// </summary>
  /// <returns>The collection of atomic values</returns>
  protected abstract IEnumerable<object?> GetEqualityComponents();

  #region Equality Members

  public bool Equals(ValueObject? other)
  {
    if (other is null) return false;
    if (ReferenceEquals(this, other)) return true;
    if (GetType() != other.GetType()) return false;

    return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
  }

  public override bool Equals(object? obj)
  {
    if (obj is null) return false;
    if (ReferenceEquals(this, obj)) return true;
    if (obj.GetType() != GetType()) return false;
    return Equals((ValueObject)obj);
  }

  public override int GetHashCode()
  {
    return GetEqualityComponents()
        .Select(x => x?.GetHashCode() ?? 0)
        .Aggregate((x, y) => x ^ y);
  }

  public static bool operator ==(ValueObject? left, ValueObject? right)
  {
    return Equals(left, right);
  }

  public static bool operator !=(ValueObject? left, ValueObject? right)
  {
    return !Equals(left, right);
  }

  #endregion
}
