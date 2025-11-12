using MediatR;

namespace UserService.Application.Features.Authentication.Commands.SignOut;

/// <summary>
/// Command to sign out a user and revoke their session.
/// </summary>
public sealed record SignOutCommand : IRequest<Unit>
{
  /// <summary>
  /// Gets the user identifier.
  /// </summary>
  public required Guid UserId { get; init; }

  /// <summary>
  /// Gets the session identifier to revoke.
  /// </summary>
  public required Guid SessionId { get; init; }
}
