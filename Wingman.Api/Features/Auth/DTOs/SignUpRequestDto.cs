using FluentValidation;
using Wingman.Api.Features.Auth.Helpers.Validators;

namespace Wingman.Api.Features.Auth.DTOs;

public class SignUpRequestDto
{
    private string? _email;
    public required string Email { get => _email!; set { _email = value.ToLower(); } }
    public required string Password { get; set; }
    public required string PasswordConfirmation { get; set; }
}

public class SignUpRequestDtoValidator : AbstractValidator<SignUpRequestDto>
{
    public SignUpRequestDtoValidator()
    {
        RuleFor(signUpDto => signUpDto.Email).EmailRules();

        RuleFor(signUpDto => signUpDto.Password).PasswordRules();

        RuleFor(signUpDto => signUpDto.PasswordConfirmation)
            .Equal(signUpDto => signUpDto.Password)
                .WithMessage("Password and confirmation password do not match.");
    }
}