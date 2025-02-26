using FluentValidation;
using Wingman.Api.Features.Auth.Helpers.Constants;

namespace Wingman.Api.Features.Auth.DTOs;

public class LoginRequestDto
{
    private string? _email;
    public required string Email { get => _email!; set { _email = value.ToLower(); } }
    public required string Password { get; set; }
}

public class LoginRequestDtoValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestDtoValidator()
    {
        RuleFor(loginDto => loginDto.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(LengthConstants.EMAIL_MAX_LENGTH);

        RuleFor(loginDto => loginDto.Password)
            .NotEmpty()
            .MinimumLength(LengthConstants.PASSWORD_MIN_LENGTH)
            .Matches(@"[A-Z]+")
            .Matches(@"[a-z]+")
            .Matches(@"[0-9]+")
            .Matches(@"[^a-zA-Z0-9]");
    }
}