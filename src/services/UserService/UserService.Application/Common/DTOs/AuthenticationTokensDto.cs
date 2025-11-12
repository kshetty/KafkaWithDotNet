namespace UserService.Application.Common.DTOs;

/// <summary>
/// DTO containing authentication tokens.
/// </summary>
public sealed record AuthenticationTokensDto
{
  /// <summary>
  /// Gets the JWT access token.
  /// </summary>
  public required string AccessToken { get; init; }

  /// <summary>
  /// Gets the refresh token.
  /// </summary>
  public required string RefreshToken { get; init; }

  /// <summary>
  /// Gets the token type (usually "Bearer").
  /// </summary>
  public required string TokenType { get; init; }

  /// <summary>
  /// Gets the access token expiry time in seconds.
  /// </summary>
  public required int ExpiresIn { get; init; }
}
