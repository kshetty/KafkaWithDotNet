using MediatR;
using UserService.Application.Common.DTOs;
using UserService.Application.Common.Interfaces.Persistence;

namespace UserService.Application.Features.Users.Queries.GetUserSessions;

/// <summary>
/// Handler for GetUserSessionsQuery.
/// </summary>
public sealed class GetUserSessionsQueryHandler : IRequestHandler<GetUserSessionsQuery, IReadOnlyList<UserSessionDto>>
{
  private readonly IUnitOfWork _unitOfWork;

  public GetUserSessionsQueryHandler(IUnitOfWork unitOfWork)
  {
    _unitOfWork = unitOfWork;
  }

  public async Task<IReadOnlyList<UserSessionDto>> Handle(GetUserSessionsQuery request, CancellationToken cancellationToken)
  {
    // Get active sessions
    var sessions = await _unitOfWork.UserSessions.GetActiveSessionsByUserIdAsync(request.UserId, cancellationToken);

    // Map to DTOs
    var sessionDtos = sessions.Select(s => new UserSessionDto
    {
      Id = s.Id,
      IpAddress = s.IpAddress,
      UserAgent = s.UserAgent,
      IsActive = s.IsActive,
      CreatedAt = s.CreatedAt,
      ExpiresAt = s.ExpiresAt,
      LastUsedAt = s.LastUsedAt
    }).ToList();

    return sessionDtos;
  }
}
