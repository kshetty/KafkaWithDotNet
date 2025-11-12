using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using UserService.Application.Features.Users.Queries.GetUserProfile;
using UserService.Application.Features.Users.Queries.GetUserSessions;
using UserService.Application.Common.DTOs;

namespace UserService.Presentation.Controllers;

/// <summary>
/// User management controller for profile and session operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : BaseController
{
  public UserController(IMediator mediator) : base(mediator)
  {
  }

  /// <summary>
  /// Get the authenticated user's profile information
  /// </summary>
  /// <returns>User profile details</returns>
  [HttpGet("profile")]
  [SwaggerOperation(Summary = "Get user profile", Description = "Returns the profile information of the authenticated user")]
  [SwaggerResponse(200, "Profile retrieved successfully", typeof(UserDto))]
  [SwaggerResponse(401, "User not authenticated")]
  [SwaggerResponse(404, "User not found")]
  [SwaggerResponse(500, "Internal server error")]
  public async Task<IActionResult> GetProfile()
  {
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
    {
      return Unauthorized(new { message = "Invalid user token" });
    }

    var query = new GetUserProfileQuery { UserId = userId };
    var result = await Mediator.Send(query);
    return HandleResult(result);
  }

  /// <summary>
  /// Get all active sessions for the authenticated user
  /// </summary>
  /// <returns>List of user sessions</returns>
  [HttpGet("sessions")]
  [SwaggerOperation(Summary = "Get user sessions", Description = "Returns all active sessions for the authenticated user")]
  [SwaggerResponse(200, "Sessions retrieved successfully", typeof(IReadOnlyList<UserSessionDto>))]
  [SwaggerResponse(401, "User not authenticated")]
  [SwaggerResponse(500, "Internal server error")]
  public async Task<IActionResult> GetSessions()
  {
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
    {
      return Unauthorized(new { message = "Invalid user token" });
    }

    var query = new GetUserSessionsQuery { UserId = userId };
    var result = await Mediator.Send(query);
    return Ok(result);
  }
}
