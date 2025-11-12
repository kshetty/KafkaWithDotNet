using MediatR;
using UserService.Application.Common.DTOs;

namespace UserService.Application.Features.Authentication.Commands.SignIn;

/// <summary>
/// Command to sign in a user.
/// </summary>
public sealed record SignInCommand : IRequest<AuthenticationResultDto>
{
  /// <summary>
  /// Gets the user's email address.
  /// </summary>
  public required string Email { get; init; }

  /// <summary>
  /// Gets the user's password.
  /// </summary>
  public required string Password { get; init; }

  /// <summary>
  /// Gets the IP address of the sign-in request.
  /// </summary>
  public string? IpAddress { get; init; }

  /// <summary>
  /// Gets the user agent of the sign-in request.
  /// </summary>
  public string? UserAgent { get; init; }
}
