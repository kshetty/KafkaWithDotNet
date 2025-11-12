using MediatR;
using UserService.Application.Common.DTOs;
using UserService.Application.Common.Interfaces.Persistence;
using UserService.Application.Common.Interfaces.Services;
using UserService.Domain.Exceptions;
using UserService.Domain.ValueObjects;

namespace UserService.Application.Features.Authentication.Commands.SignIn;

/// <summary>
/// Handler for SignInCommand.
/// </summary>
public sealed class SignInCommandHandler : IRequestHandler<SignInCommand, AuthenticationResultDto>
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly ITokenService _tokenService;
  private readonly IEventPublisher _eventPublisher;

  public SignInCommandHandler(
      IUnitOfWork unitOfWork,
      ITokenService tokenService,
      IEventPublisher eventPublisher)
  {
    _unitOfWork = unitOfWork;
    _tokenService = tokenService;
    _eventPublisher = eventPublisher;
  }

  public async Task<AuthenticationResultDto> Handle(SignInCommand request, CancellationToken cancellationToken)
  {
    // Create email value object
    var email = Email.Create(request.Email);

    // Get user by email
    var user = await _unitOfWork.Users.GetByEmailAsync(email, cancellationToken)
        ?? throw new UserNotFoundException(email.Value);

    // Check if account is locked
    if (user.IsLockedOut())
    {
      throw new InvalidOperationException("Account is temporarily locked due to multiple failed login attempts. Please try again later.");
    }

    // Check if account is active
    if (!user.IsActive)
    {
      throw new InvalidOperationException("Account is inactive. Please contact support.");
    }

    // Verify password
    if (!user.VerifyPassword(request.Password))
    {
      // Record failed login attempt
      user.RecordFailedLogin();
      await _unitOfWork.SaveChangesAsync(cancellationToken);
      throw new InvalidPasswordException();
    }

    // Record successful login and create session
    var session = user.RecordSuccessfulLogin(request.IpAddress, request.UserAgent);

    // Save changes
    await _unitOfWork.SaveChangesAsync(cancellationToken);

    // Publish domain events to Kafka
    await _eventPublisher.PublishManyAsync(user.DomainEvents, cancellationToken);

    // Generate JWT token
    var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email.Value, user.FullName);

    // Map to DTOs
    var userDto = new UserDto
    {
      Id = user.Id,
      Email = user.Email.Value,
      FullName = user.FullName,
      IsEmailVerified = user.IsEmailVerified,
      IsActive = user.IsActive,
      CreatedAt = user.CreatedAt,
      LastLoginAt = user.LastLoginAt
    };

    var tokensDto = new AuthenticationTokensDto
    {
      AccessToken = accessToken,
      RefreshToken = session.RefreshToken,
      TokenType = "Bearer",
      ExpiresIn = 3600 // 1 hour in seconds
    };

    return new AuthenticationResultDto
    {
      User = userDto,
      Tokens = tokensDto
    };
  }
}
