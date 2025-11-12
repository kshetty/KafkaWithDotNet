namespace UserService.Domain.Exceptions;

/// <summary>
/// Exception thrown when attempting to register a user with an email that already exists.
/// </summary>
public sealed class DuplicateEmailException : DomainException
{
  /// <summary>
  /// Initializes a new instance of the DuplicateEmailException class.
  /// </summary>
  /// <param name="email">The duplicate email address</param>
  public DuplicateEmailException(string email)
      : base("DUPLICATE_EMAIL", $"A user with email '{email}' already exists.")
  {
    Email = email;
  }

  /// <summary>
  /// Gets the duplicate email address.
  /// </summary>
  public string Email { get; }
}
