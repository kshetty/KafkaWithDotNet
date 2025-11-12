namespace UserService.Domain.Exceptions;

/// <summary>
/// Base exception for all domain-specific exceptions.
/// Domain exceptions represent violations of business rules.
/// </summary>
public abstract class DomainException : Exception
{
  /// <summary>
  /// Gets the error code for this exception.
  /// </summary>
  public string ErrorCode { get; }

  /// <summary>
  /// Initializes a new instance of the DomainException class.
  /// </summary>
  /// <param name="errorCode">The error code</param>
  /// <param name="message">The error message</param>
  protected DomainException(string errorCode, string message)
      : base(message)
  {
    ErrorCode = errorCode;
  }

  /// <summary>
  /// Initializes a new instance of the DomainException class with an inner exception.
  /// </summary>
  /// <param name="errorCode">The error code</param>
  /// <param name="message">The error message</param>
  /// <param name="innerException">The inner exception</param>
  protected DomainException(string errorCode, string message, Exception innerException)
      : base(message, innerException)
  {
    ErrorCode = errorCode;
  }
}
