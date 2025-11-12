using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using UserService.Application.Features.Authentication.Commands.RefreshToken;
using UserService.Application.Features.Authentication.Commands.SignIn;
using UserService.Application.Features.Authentication.Commands.SignOut;
using UserService.Application.Features.Authentication.Commands.SignUp;
using UserService.Application.Common.DTOs;

namespace UserService.Presentation.Controllers;

/// <summary>
/// Authentication controller for user registration, login, logout, and token refresh
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : BaseController
{
  public AuthController(IMediator mediator) : base(mediator)
  {
  }

  /// <summary>
  /// Register a new user account
  /// </summary>
  /// <param name="command">User registration details</param>
  /// <returns>Authentication tokens and user information</returns>
  [HttpPost("signup")]
  [AllowAnonymous]
  [SwaggerOperation(Summary = "Register a new user", Description = "Creates a new user account and returns authentication tokens")]
  [SwaggerResponse(201, "User registered successfully", typeof(AuthenticationResultDto))]
  [SwaggerResponse(400, "Invalid request data or email already exists")]
  [SwaggerResponse(500, "Internal server error")]
  public async Task<IActionResult> SignUp([FromBody] SignUpCommand command)
  {
    var result = await Mediator.Send(command);
    return Created(result);
  }

  /// <summary>
  /// Authenticate a user and obtain access tokens
  /// </summary>
  /// <param name="command">User credentials</param>
  /// <returns>Authentication tokens and user information</returns>
  [HttpPost("signin")]
  [AllowAnonymous]
  [SwaggerOperation(Summary = "Sign in", Description = "Authenticates a user with email and password, returns JWT tokens")]
  [SwaggerResponse(200, "User authenticated successfully", typeof(AuthenticationResultDto))]
  [SwaggerResponse(400, "Invalid credentials")]
  [SwaggerResponse(401, "Account is locked or inactive")]
  [SwaggerResponse(500, "Internal server error")]
  public async Task<IActionResult> SignIn([FromBody] SignInCommand command)
  {
    var result = await Mediator.Send(command);
    return Ok(result);
  }

  /// <summary>
  /// Sign out the current user and revoke their refresh token
  /// </summary>
  /// <param name="command">Refresh token to revoke</param>
  /// <returns>Success confirmation</returns>
  [HttpPost("signout")]
  [Authorize]
  [SwaggerOperation(Summary = "Sign out", Description = "Revokes the user's refresh token and signs them out")]
  [SwaggerResponse(200, "User signed out successfully")]
  [SwaggerResponse(400, "Invalid refresh token")]
  [SwaggerResponse(401, "User not authenticated")]
  [SwaggerResponse(500, "Internal server error")]
  public async Task<IActionResult> SignOut([FromBody] SignOutCommand command)
  {
    await Mediator.Send(command);
    return Ok(new { message = "Signed out successfully" });
  }

  /// <summary>
  /// Refresh access token using a valid refresh token
  /// </summary>
  /// <param name="command">Refresh token details</param>
  /// <returns>New access and refresh tokens</returns>
  [HttpPost("refresh")]
  [AllowAnonymous]
  [SwaggerOperation(Summary = "Refresh token", Description = "Generates new access and refresh tokens using a valid refresh token")]
  [SwaggerResponse(200, "Tokens refreshed successfully", typeof(AuthenticationResultDto))]
  [SwaggerResponse(400, "Invalid or expired refresh token")]
  [SwaggerResponse(500, "Internal server error")]
  public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
  {
    var result = await Mediator.Send(command);
    return Ok(result);
  }
}
