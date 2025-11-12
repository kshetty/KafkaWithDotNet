namespace UserService.Domain.Exceptions;

/// <summary>
/// Exception thrown when a user session is not found or has expired.
/// </summary>
public sealed class InvalidSessionException : DomainException
{
  /// <summary>
  /// Initializes a new instance of the InvalidSessionException class.
  /// </summary>
  /// <param name="sessionId">The session identifier</param>
  public InvalidSessionException(Guid sessionId)
      : base("INVALID_SESSION", $"Session with ID '{sessionId}' is invalid or has expired.")
  {
    SessionId = sessionId;
  }

  /// <summary>
  /// Initializes a new instance of the InvalidSessionException class with a custom message.
  /// </summary>
  /// <param name="message">The error message</param>
  public InvalidSessionException(string message)
      : base("INVALID_SESSION", message)
  {
  }

  /// <summary>
  /// Gets the session identifier if specified.
  /// </summary>
  public Guid? SessionId { get; }
}
