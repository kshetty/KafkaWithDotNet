namespace UserService.Domain.Exceptions;

/// <summary>
/// Exception thrown when a user is not found.
/// </summary>
public sealed class UserNotFoundException : DomainException
{
  /// <summary>
  /// Initializes a new instance of the UserNotFoundException class.
  /// </summary>
  /// <param name="userId">The user identifier</param>
  public UserNotFoundException(Guid userId)
      : base("USER_NOT_FOUND", $"User with ID '{userId}' was not found.")
  {
    UserId = userId;
  }

  /// <summary>
  /// Initializes a new instance of the UserNotFoundException class by email.
  /// </summary>
  /// <param name="email">The user email</param>
  public UserNotFoundException(string email)
      : base("USER_NOT_FOUND", $"User with email '{email}' was not found.")
  {
    Email = email;
  }

  /// <summary>
  /// Gets the user identifier if specified.
  /// </summary>
  public Guid? UserId { get; }

  /// <summary>
  /// Gets the user email if specified.
  /// </summary>
  public string? Email { get; }
}
