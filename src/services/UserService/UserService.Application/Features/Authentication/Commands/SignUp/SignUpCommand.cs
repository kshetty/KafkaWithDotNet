using MediatR;
using UserService.Application.Common.DTOs;

namespace UserService.Application.Features.Authentication.Commands.SignUp;

/// <summary>
/// Command to register a new user.
/// </summary>
public sealed record SignUpCommand : IRequest<AuthenticationResultDto>
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
  /// Gets the user's full name.
  /// </summary>
  public required string FullName { get; init; }
}
