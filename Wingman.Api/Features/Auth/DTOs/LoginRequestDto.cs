using FluentValidation;
using Wingman.Api.Features.Auth.Helpers.Validators;

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
        RuleFor(loginDto => loginDto.Email).EmailRules();
        RuleFor(loginDto => loginDto.Password).PasswordRules();
    }
}