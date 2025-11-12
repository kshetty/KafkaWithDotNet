using FluentValidation;

namespace UserService.Application.Features.Authentication.Commands.SignIn;

/// <summary>
/// Validator for SignInCommand.
/// </summary>
public sealed class SignInCommandValidator : AbstractValidator<SignInCommand>
{
  public SignInCommandValidator()
  {
    RuleFor(x => x.Email)
        .NotEmpty().WithMessage("Email is required.")
        .EmailAddress().WithMessage("Email format is invalid.");

    RuleFor(x => x.Password)
        .NotEmpty().WithMessage("Password is required.");

    RuleFor(x => x.IpAddress)
        .MaximumLength(45).WithMessage("IP address cannot exceed 45 characters.")
        .When(x => !string.IsNullOrEmpty(x.IpAddress));

    RuleFor(x => x.UserAgent)
        .MaximumLength(500).WithMessage("User agent cannot exceed 500 characters.")
        .When(x => !string.IsNullOrEmpty(x.UserAgent));
  }
}
