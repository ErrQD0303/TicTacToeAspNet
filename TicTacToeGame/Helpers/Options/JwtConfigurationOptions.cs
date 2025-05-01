namespace TicTacToeGame.Helpers.Options;

public class JwtConfigurationOptions
{
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string IssuerSigningKey { get; set; } = string.Empty;
    public int Expiration { get; set; } = 3600; // 1 hour
    public string TokenType { get; set; } = string.Empty;
}