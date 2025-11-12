using MediatR;
using UserService.Application.Common.DTOs;

namespace UserService.Application.Features.Users.Queries.GetUserSessions;

/// <summary>
/// Query to get all active sessions for a user.
/// </summary>
public sealed record GetUserSessionsQuery : IRequest<IReadOnlyList<UserSessionDto>>
{
  /// <summary>
  /// Gets the user identifier.
  /// </summary>
  public required Guid UserId { get; init; }
}
