namespace UserService.Domain.Exceptions;

/// <summary>
/// Exception thrown when a password does not meet the required strength criteria.
/// </summary>
public sealed class WeakPasswordException : DomainException
{
  /// <summary>
  /// Initializes a new instance of the WeakPasswordException class.
  /// </summary>
  /// <param name="requirements">The list of requirements that were not met</param>
  public WeakPasswordException(params string[] requirements)
      : base("WEAK_PASSWORD", BuildMessage(requirements))
  {
    Requirements = requirements;
  }

  /// <summary>
  /// Gets the list of password requirements that were not met.
  /// </summary>
  public string[] Requirements { get; }

  private static string BuildMessage(string[] requirements)
  {
    if (requirements.Length == 0)
    {
      return "Password does not meet the required strength criteria.";
    }

    var requirementsList = string.Join(", ", requirements);
    return $"Password does not meet the required strength criteria. Missing requirements: {requirementsList}";
  }
}
