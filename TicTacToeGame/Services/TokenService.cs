using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using TicTacToeGame.Helpers;
using TicTacToeGame.Helpers.Options;
using TicTacToeGame.Models;
using TicTacToeGame.Services.Interfaces;

namespace TicTacToeGame.Services;

public class TokenService : ITokenService
{
    public JwtConfigurationOptions JwtOptions { get; private set; }

    public TokenService(IOptions<JwtConfigurationOptions> jwtOptions)
    {
        JwtOptions = jwtOptions.Value;
    }
    public string GenerateAccessToken(AppUser user, int? expiration = null)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.GivenName, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
        };
        return JwtGenerator.GenerateAccessToken(JwtOptions.Issuer, JwtOptions.Audience, JwtOptions.IssuerSigningKey, expiration ?? JwtOptions.Expiration, claims, null);
    }

    public string GenerateRefreshToken(int length = 32)
    {
        return JwtGenerator.GenerateRefreshToken(length);
    }
}