using MediatR;
using UserService.Application.Common.DTOs;

namespace UserService.Application.Features.Users.Queries.GetUserProfile;

/// <summary>
/// Query to get a user's profile information.
/// </summary>
public sealed record GetUserProfileQuery : IRequest<UserDto>
{
  /// <summary>
  /// Gets the user identifier.
  /// </summary>
  public required Guid UserId { get; init; }
}
