using DistLib;
using FluentValidation;

namespace PFinance.Services.Identity.Application.UserAccount.Commands.SignInPassword;

public sealed record SignInPassword(string Email, string Password) : ICommand<Result<AuthDto>>;

public sealed class SignInPasswordValidator : AbstractValidator<SignInPassword>
{
    public SignInPasswordValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .MaximumLength(100).WithMessage("Password cannot exceed 100 characters.");
    }
}
