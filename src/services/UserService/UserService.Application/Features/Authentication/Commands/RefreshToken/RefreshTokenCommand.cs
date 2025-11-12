using MediatR;
using UserService.Application.Common.DTOs;

namespace UserService.Application.Features.Authentication.Commands.RefreshToken;

/// <summary>
/// Command to refresh an access token using a refresh token.
/// </summary>
public sealed record RefreshTokenCommand : IRequest<AuthenticationTokensDto>
{
  /// <summary>
  /// Gets the refresh token.
  /// </summary>
  public required string RefreshToken { get; init; }
}
