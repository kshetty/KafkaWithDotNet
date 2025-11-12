using FluentValidation;

namespace UserService.Application.Features.Users.Queries.GetUserProfile;

/// <summary>
/// Validator for GetUserProfileQuery.
/// </summary>
public sealed class GetUserProfileQueryValidator : AbstractValidator<GetUserProfileQuery>
{
  public GetUserProfileQueryValidator()
  {
    RuleFor(x => x.UserId)
        .NotEmpty().WithMessage("User ID is required.");
  }
}
