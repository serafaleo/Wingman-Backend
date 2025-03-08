using FluentValidation;
using Wingman.Api.Features.Auth.Helpers.Constants;

namespace Wingman.Api.Features.Auth.Helpers.Validators;

public static class CommonFluentValidators
{
    public static IRuleBuilderOptions<T, string> EmailRules<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Email must not be empty.")
            .EmailAddress().WithMessage("Email must be a valid email address.")
            .MaximumLength(LengthConstants.EMAIL_MAX_LENGTH).WithMessage($"Email must be up to {LengthConstants.EMAIL_MAX_LENGTH} characters long.");
    }

    public static IRuleBuilderOptions<T, string> PasswordRules<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Password must not be empty.")
            .MinimumLength(LengthConstants.PASSWORD_MIN_LENGTH).WithMessage($"Password must be at least {LengthConstants.PASSWORD_MIN_LENGTH} characters long.")
            .Matches(@"[A-Z]+").WithMessage("Password must have at least one uppercase letter.")
            .Matches(@"[a-z]+").WithMessage("Password must have at least one lowercase letter.")
            .Matches(@"[0-9]+").WithMessage("Password must have at least one numeric digit.")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must have at least one special character (non-alphanumeric).");
    }
}
