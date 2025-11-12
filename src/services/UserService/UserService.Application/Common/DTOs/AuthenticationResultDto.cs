namespace UserService.Application.Common.DTOs;

/// <summary>
/// DTO containing authentication result with user info and tokens.
/// </summary>
public sealed record AuthenticationResultDto
{
  /// <summary>
  /// Gets the user information.
  /// </summary>
  public required UserDto User { get; init; }

  /// <summary>
  /// Gets the authentication tokens.
  /// </summary>
  public required AuthenticationTokensDto Tokens { get; init; }
}
