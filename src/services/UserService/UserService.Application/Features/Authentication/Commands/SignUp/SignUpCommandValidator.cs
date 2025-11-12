using FluentValidation;

namespace UserService.Application.Features.Authentication.Commands.SignUp;

/// <summary>
/// Validator for SignUpCommand.
/// </summary>
public sealed class SignUpCommandValidator : AbstractValidator<SignUpCommand>
{
  public SignUpCommandValidator()
  {
    RuleFor(x => x.Email)
        .NotEmpty().WithMessage("Email is required.")
        .MaximumLength(255).WithMessage("Email cannot exceed 255 characters.")
        .EmailAddress().WithMessage("Email format is invalid.");

    RuleFor(x => x.Password)
        .NotEmpty().WithMessage("Password is required.")
        .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
        .MaximumLength(128).WithMessage("Password cannot exceed 128 characters.")
        .Matches(@"[0-9]").WithMessage("Password must contain at least one digit.")
        .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
        .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
        .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

    RuleFor(x => x.FullName)
        .NotEmpty().WithMessage("Full name is required.")
        .MaximumLength(200).WithMessage("Full name cannot exceed 200 characters.")
        .Matches(@"^[\p{L}\p{M}\s'-]+$").WithMessage("Full name contains invalid characters.");
  }
}
