using System.Text.Json.Serialization;

namespace TicTacToeGame.Models.Requests.Users;

public class UserLoginRequest
{
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;
    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}