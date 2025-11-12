namespace UserService.Application.Common.DTOs;

/// <summary>
/// DTO representing user information.
/// </summary>
public sealed record UserDto
{
  /// <summary>
  /// Gets the user identifier.
  /// </summary>
  public required Guid Id { get; init; }

  /// <summary>
  /// Gets the user email.
  /// </summary>
  public required string Email { get; init; }

  /// <summary>
  /// Gets the user's full name.
  /// </summary>
  public required string FullName { get; init; }

  /// <summary>
  /// Gets whether the email is verified.
  /// </summary>
  public required bool IsEmailVerified { get; init; }

  /// <summary>
  /// Gets whether the account is active.
  /// </summary>
  public required bool IsActive { get; init; }

  /// <summary>
  /// Gets the date the user was created.
  /// </summary>
  public required DateTime CreatedAt { get; init; }

  /// <summary>
  /// Gets the date the user last logged in.
  /// </summary>
  public DateTime? LastLoginAt { get; init; }
}
