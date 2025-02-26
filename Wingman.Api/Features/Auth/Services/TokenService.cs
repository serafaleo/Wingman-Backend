using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Wingman.Api.Features.Auth.Helpers.Objects;
using Wingman.Api.Features.Auth.Models;
using Wingman.Api.Features.Auth.Services.Interfaces;

namespace Wingman.Api.Features.Auth.Services;

public class TokenService(IConfiguration configuration) : ITokenService
{
    private readonly IConfiguration _configuration = configuration;

    public string GenerateAccessToken(User user)
    {
        List<Claim> claims = [new Claim(ClaimTypes.Email, user.Email!),
                              new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())];

        SymmetricSecurityKey secretKey = new(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        SigningCredentials signinCredentials = new(secretKey, SecurityAlgorithms.HmacSha512);

        JwtSecurityToken tokeOptions = new(
            issuer: _configuration["Jwt:ValidIssuer"],
            audience: _configuration["Jwt:ValidAudience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:TokenValidityInMinutes"]!)),
            signingCredentials: signinCredentials
        );

        return new JwtSecurityTokenHandler().WriteToken(tokeOptions);
    }

    public RefreshToken GenerateRefreshToken()
    {
        byte[] randomNumber = new byte[32];
        using (RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create())
        {
            randomNumberGenerator.GetBytes(randomNumber);

            return new()
            {
                Token = Convert.ToBase64String(randomNumber),
                Expiration = DateTime.UtcNow.AddDays(int.Parse(_configuration["Jwt:RefreshTokenValidityInDays"]!))
            };
        }
    }
}
