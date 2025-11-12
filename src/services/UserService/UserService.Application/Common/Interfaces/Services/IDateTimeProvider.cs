namespace UserService.Application.Common.Interfaces.Services;

/// <summary>
/// Service interface for getting current date and time.
/// Useful for testing and time-based operations.
/// </summary>
public interface IDateTimeProvider
{
  /// <summary>
  /// Gets the current UTC date and time.
  /// </summary>
  DateTime UtcNow { get; }
}
