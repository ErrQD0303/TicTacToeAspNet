using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace TicTacToeGame.Helpers;

public class JwtGenerator
{
    public static string GenerateAccessToken(string issuer, string audience, string signingKey, int expiration, IEnumerable<Claim> claims, Dictionary<string, object>? additionalClaims = null)
    {
        var jwk = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        var jsonWebKey = new JsonWebKey
        {
            Kty = "oct",
            K = Base64UrlEncoder.Encode(jwk.Key),
        };
        var signingCredentials = new SigningCredentials(jsonWebKey, SecurityAlgorithms.HmacSha256);

        var jwtHeader = new JwtHeader(signingCredentials);

        additionalClaims ??= [];

        var jwtPayload = new JwtPayload(issuer, audience, claims, additionalClaims, DateTime.UtcNow, DateTime.UtcNow.AddSeconds(expiration), DateTime.UtcNow);

        var token = new JwtSecurityToken(jwtHeader, jwtPayload);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}