using MediatR;
using UserService.Application.Common.DTOs;
using UserService.Application.Common.Interfaces.Persistence;
using UserService.Application.Common.Interfaces.Services;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using UserService.Domain.ValueObjects;

namespace UserService.Application.Features.Authentication.Commands.SignUp;

/// <summary>
/// Handler for SignUpCommand.
/// </summary>
public sealed class SignUpCommandHandler : IRequestHandler<SignUpCommand, AuthenticationResultDto>
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly ITokenService _tokenService;
  private readonly IEventPublisher _eventPublisher;

  public SignUpCommandHandler(
      IUnitOfWork unitOfWork,
      ITokenService tokenService,
      IEventPublisher eventPublisher)
  {
    _unitOfWork = unitOfWork;
    _tokenService = tokenService;
    _eventPublisher = eventPublisher;
  }

  public async Task<AuthenticationResultDto> Handle(SignUpCommand request, CancellationToken cancellationToken)
  {
    // Create email value object
    var email = Email.Create(request.Email);

    // Check if user already exists
    if (await _unitOfWork.Users.ExistsAsync(email, cancellationToken))
    {
      throw new DuplicateEmailException(email.Value);
    }

    // Create password value object (will be hashed)
    var password = Password.Create(request.Password);

    // Create user entity
    var user = User.Create(email, password, request.FullName);

    // Add user to repository
    await _unitOfWork.Users.AddAsync(user, cancellationToken);

    // Create initial session
    var session = user.RecordSuccessfulLogin(null, null);

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
