using UserService.Application.Common.Interfaces.Services;

namespace UserService.Infrastructure.Services;

/// <summary>
/// Provides current date/time in UTC for application use.
/// Abstraction allows for deterministic time in tests.
/// </summary>
public sealed class DateTimeProvider : IDateTimeProvider
{
  public DateTime UtcNow => DateTime.UtcNow;
}
