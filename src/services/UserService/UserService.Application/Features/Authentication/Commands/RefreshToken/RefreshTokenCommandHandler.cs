using MediatR;
using UserService.Application.Common.DTOs;
using UserService.Application.Common.Interfaces.Persistence;
using UserService.Application.Common.Interfaces.Services;
using UserService.Domain.Exceptions;

namespace UserService.Application.Features.Authentication.Commands.RefreshToken;

/// <summary>
/// Handler for RefreshTokenCommand.
/// </summary>
public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthenticationTokensDto>
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly ITokenService _tokenService;

  public RefreshTokenCommandHandler(
      IUnitOfWork unitOfWork,
      ITokenService tokenService)
  {
    _unitOfWork = unitOfWork;
    _tokenService = tokenService;
  }

  public async Task<AuthenticationTokensDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
  {
    // Get session by refresh token
    var session = await _unitOfWork.UserSessions.GetByRefreshTokenAsync(request.RefreshToken, cancellationToken)
        ?? throw new InvalidSessionException("Invalid or expired refresh token.");

    // Validate session
    if (!session.IsValid())
    {
      throw new InvalidSessionException("Session has expired or been revoked.");
    }

    // Get user
    var user = await _unitOfWork.Users.GetByIdAsync(session.UserId, cancellationToken)
        ?? throw new UserNotFoundException(session.UserId);

    // Check if account is active
    if (!user.IsActive)
    {
      throw new InvalidOperationException("Account is inactive.");
    }

    // Refresh the session (generates new refresh token)
    session.Refresh();

    // Save changes
    await _unitOfWork.SaveChangesAsync(cancellationToken);

    // Generate new JWT access token
    var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email.Value, user.FullName);

    return new AuthenticationTokensDto
    {
      AccessToken = accessToken,
      RefreshToken = session.RefreshToken,
      TokenType = "Bearer",
      ExpiresIn = 3600 // 1 hour in seconds
    };
  }
}
