using MediatR;
using UserService.Application.Common.DTOs;
using UserService.Application.Common.Interfaces.Persistence;
using UserService.Application.Common.Interfaces.Services;
using UserService.Domain.Exceptions;

namespace UserService.Application.Features.Users.Queries.GetUserProfile;

/// <summary>
/// Handler for GetUserProfileQuery.
/// </summary>
public sealed class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserDto>
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly ICacheService _cacheService;

  public GetUserProfileQueryHandler(
      IUnitOfWork unitOfWork,
      ICacheService cacheService)
  {
    _unitOfWork = unitOfWork;
    _cacheService = cacheService;
  }

  public async Task<UserDto> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
  {
    // Try to get from cache first
    var cacheKey = $"user:profile:{request.UserId}";
    var cachedUser = await _cacheService.GetAsync<UserDto>(cacheKey, cancellationToken);

    if (cachedUser is not null)
    {
      return cachedUser;
    }

    // Get from database
    var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken)
        ?? throw new UserNotFoundException(request.UserId);

    // Map to DTO
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

    // Cache for 5 minutes
    await _cacheService.SetAsync(cacheKey, userDto, TimeSpan.FromMinutes(5), cancellationToken);

    return userDto;
  }
}
