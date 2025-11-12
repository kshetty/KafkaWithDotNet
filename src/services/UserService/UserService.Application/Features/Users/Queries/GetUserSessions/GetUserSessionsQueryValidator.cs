using FluentValidation;

namespace UserService.Application.Features.Users.Queries.GetUserSessions;

/// <summary>
/// Validator for GetUserSessionsQuery.
/// </summary>
public sealed class GetUserSessionsQueryValidator : AbstractValidator<GetUserSessionsQuery>
{
  public GetUserSessionsQueryValidator()
  {
    RuleFor(x => x.UserId)
        .NotEmpty().WithMessage("User ID is required.");
  }
}
