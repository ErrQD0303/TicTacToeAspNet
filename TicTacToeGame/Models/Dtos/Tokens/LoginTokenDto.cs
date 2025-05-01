using System.Text.Json.Serialization;

namespace TicTacToeGame.Models.Dtos.Tokens;

public class LoginTokenDto
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;
    // [JsonPropertyName("refresh_token")]
    // public string RefreshToken { get; set; } = string.Empty;
}