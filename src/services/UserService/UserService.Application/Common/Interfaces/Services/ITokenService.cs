namespace UserService.Application.Common.Interfaces.Services;

/// <summary>
/// Service interface for JWT token generation and validation.
/// </summary>
public interface ITokenService
{
  /// <summary>
  /// Generates a JWT access token for a user.
  /// </summary>
  /// <param name="userId">The user identifier</param>
  /// <param name="email">The user email</param>
  /// <param name="fullName">The user's full name</param>
  /// <returns>The generated access token</returns>
  string GenerateAccessToken(Guid userId, string email, string fullName);

  /// <summary>
  /// Validates a JWT access token.
  /// </summary>
  /// <param name="token">The token to validate</param>
  /// <returns>True if valid, false otherwise</returns>
  bool ValidateToken(string token);

  /// <summary>
  /// Gets the user identifier from a token.
  /// </summary>
  /// <param name="token">The JWT token</param>
  /// <returns>The user identifier if valid, null otherwise</returns>
  Guid? GetUserIdFromToken(string token);
}
