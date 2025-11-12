using FluentValidation;

namespace UserService.Application.Features.Authentication.Commands.SignOut;

/// <summary>
/// Validator for SignOutCommand.
/// </summary>
public sealed class SignOutCommandValidator : AbstractValidator<SignOutCommand>
{
  public SignOutCommandValidator()
  {
    RuleFor(x => x.UserId)
        .NotEmpty().WithMessage("User ID is required.");

    RuleFor(x => x.SessionId)
        .NotEmpty().WithMessage("Session ID is required.");
  }
}
