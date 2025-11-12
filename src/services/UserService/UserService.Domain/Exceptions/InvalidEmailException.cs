namespace UserService.Domain.Exceptions;

/// <summary>
/// Exception thrown when an email address is invalid.
/// </summary>
public sealed class InvalidEmailException : DomainException
{
  /// <summary>
  /// Initializes a new instance of the InvalidEmailException class.
  /// </summary>
  /// <param name="email">The invalid email address</param>
  /// <param name="reason">The reason why the email is invalid</param>
  public InvalidEmailException(string email, string? reason = null)
      : base("INVALID_EMAIL", BuildMessage(email, reason))
  {
    Email = email;
    Reason = reason;
  }

  /// <summary>
  /// Gets the invalid email address.
  /// </summary>
  public string Email { get; }

  /// <summary>
  /// Gets the reason why the email is invalid.
  /// </summary>
  public string? Reason { get; }

  private static string BuildMessage(string email, string? reason)
  {
    var baseMessage = $"Email address '{email}' is invalid.";
    return reason is not null ? $"{baseMessage} {reason}" : baseMessage;
  }
}
