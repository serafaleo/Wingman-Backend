using FluentValidation;
using Wingman.Api.Features.Auth.Helpers.Constants;

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
        RuleFor(signUpDto => signUpDto.Email)
            .NotEmpty().WithMessage("Email precisa ser preenchido.")
            .EmailAddress().WithMessage("Email preenchido é inválido.")
            .MaximumLength(LengthConstants.EMAIL_MAX_LENGTH).WithMessage($"Email aceita um máximo de {LengthConstants.EMAIL_MAX_LENGTH} caracteres.");

        RuleFor(signUpDto => signUpDto.Password)
            .NotEmpty().WithMessage("Senha precisa ser preenchida.")
            .MinimumLength(LengthConstants.PASSWORD_MIN_LENGTH).WithMessage($"A senha precisa ter no mínimo {LengthConstants.PASSWORD_MIN_LENGTH} caracteres.")
            .Matches(@"[A-Z]+").WithMessage("A senha precisa conter pelo menos uma letra maiúscula.")
            .Matches(@"[a-z]+").WithMessage("A senha precisa conter pelo menos uma letra minúscula.")
            .Matches(@"[0-9]+").WithMessage("A senha precisa conter pelo menos um dígito.")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("A senha precisa conter pelo menos um caractere especial.");

        RuleFor(signUpDto => signUpDto.PasswordConfirmation)
            .Equal(signUpDto => signUpDto.Password).WithMessage("As senhas são diferentes.");
    }
}