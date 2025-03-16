using FluentValidation;
using Wingman.Api.Features.Auth.Helpers.Constants;

namespace Wingman.Api.Features.Auth.DTOs;

public class RefreshRequestDto
{
    public Guid UserId { get; set; }
    public required string RefreshToken { get; set; }
}

public class RefreshRequestDtoValidator : AbstractValidator<RefreshRequestDto>
{
    public RefreshRequestDtoValidator()
    {
        // TODO(serafa.leo): Decide whether or not to show messages and which messages to show.

        RuleFor(refreshDto => refreshDto.UserId)
            .NotEmpty().WithMessage("User ID not provided.");

        RuleFor(refreshDto => refreshDto.RefreshToken)
            .NotEmpty().WithMessage("Refresh Token not provided.")
            .Length(LengthConstants.REFRESH_TOKEN_LENGTH)
                .WithMessage($"Refresh Token must be {LengthConstants.REFRESH_TOKEN_LENGTH} characters long.");
    }
}