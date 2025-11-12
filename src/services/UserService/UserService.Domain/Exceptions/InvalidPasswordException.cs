namespace UserService.Domain.Exceptions;

/// <summary>
/// Exception thrown when an invalid password is provided during authentication.
/// </summary>
public sealed class InvalidPasswordException : DomainException
{
  /// <summary>
  /// Initializes a new instance of the InvalidPasswordException class.
  /// </summary>
  public InvalidPasswordException()
      : base("INVALID_PASSWORD", "The provided password is incorrect.")
  {
  }
}
