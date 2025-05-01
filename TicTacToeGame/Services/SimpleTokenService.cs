using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using TicTacToeGame.Helpers;
using TicTacToeGame.Helpers.Options;
using TicTacToeGame.Models;
using TicTacToeGame.Services.Interfaces;

namespace TicTacToeGame.Services;

public class SimpleTokenService : ITokenService<SimpleUser>
{
    public JwtConfigurationOptions JwtOptions { get; private set; }

    public SimpleTokenService(IOptions<JwtConfigurationOptions> jwtOptions)
    {
        JwtOptions = jwtOptions.Value;
    }
    public string GenerateAccessToken(SimpleUser user, int? expiration = null)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
        };
        return JwtGenerator.GenerateAccessToken(JwtOptions.Issuer, JwtOptions.Audience, JwtOptions.IssuerSigningKey, expiration ?? JwtOptions.Expiration, claims, null);
    }

    public string GenerateRefreshToken(int length = 32)
    {
        return JwtGenerator.GenerateRefreshToken(length);
    }
}