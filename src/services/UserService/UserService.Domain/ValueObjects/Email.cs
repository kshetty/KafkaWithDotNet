using System.Text.RegularExpressions;
using UserService.Domain.Primitives;

namespace UserService.Domain.ValueObjects;

/// <summary>
/// Value object representing an email address.
/// Ensures email addresses are valid and normalized.
/// </summary>
public sealed class Email : ValueObject
{
  // RFC 5322 compliant email regex (simplified)
  private static readonly Regex EmailRegex = new(
      @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$",
      RegexOptions.Compiled | RegexOptions.IgnoreCase);

  private const int MaxLength = 255;

  /// <summary>
  /// Gets the email address value.
  /// </summary>
  public string Value { get; }

  /// <summary>
  /// Private constructor for EF Core and internal use.
  /// </summary>
  private Email(string value)
  {
    Value = value;
  }

  /// <summary>
  /// Creates a new Email value object with validation.
  /// </summary>
  /// <param name="email">The email address string</param>
  /// <returns>A new Email instance</returns>
  /// <exception cref="ArgumentException">Thrown when the email is invalid</exception>
  public static Email Create(string email)
  {
    if (string.IsNullOrWhiteSpace(email))
    {
      throw new ArgumentException("Email cannot be empty or whitespace.", nameof(email));
    }

    var normalizedEmail = email.Trim().ToLowerInvariant();

    if (normalizedEmail.Length > MaxLength)
    {
      throw new ArgumentException($"Email cannot exceed {MaxLength} characters.", nameof(email));
    }

    if (!EmailRegex.IsMatch(normalizedEmail))
    {
      throw new ArgumentException("Email format is invalid.", nameof(email));
    }

    // Additional domain validation
    var parts = normalizedEmail.Split('@');
    if (parts.Length != 2 || parts[0].Length == 0 || parts[1].Length == 0)
    {
      throw new ArgumentException("Email must contain exactly one '@' character with content before and after.", nameof(email));
    }

    return new Email(normalizedEmail);
  }

  /// <summary>
  /// Tries to create a new Email value object without throwing exceptions.
  /// </summary>
  /// <param name="email">The email address string</param>
  /// <param name="result">The created Email instance if successful</param>
  /// <returns>True if the email was created successfully, false otherwise</returns>
  public static bool TryCreate(string email, out Email? result)
  {
    try
    {
      result = Create(email);
      return true;
    }
    catch
    {
      result = null;
      return false;
    }
  }

  /// <summary>
  /// Validates if a string is a valid email without creating an instance.
  /// </summary>
  /// <param name="email">The email address string to validate</param>
  /// <returns>True if valid, false otherwise</returns>
  public static bool IsValid(string email)
  {
    return TryCreate(email, out _);
  }

  /// <summary>
  /// Implicit conversion from Email to string.
  /// </summary>
  public static implicit operator string(Email email) => email.Value;

  /// <summary>
  /// Returns the email address as a string.
  /// </summary>
  public override string ToString() => Value;

  protected override IEnumerable<object?> GetEqualityComponents()
  {
    yield return Value;
  }
}
