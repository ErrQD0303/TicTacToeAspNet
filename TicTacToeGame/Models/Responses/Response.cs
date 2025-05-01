using System.Text.Json.Serialization;

namespace TicTacToeGame.Models.Responses;

public class Response
{
    /// <summary>
    /// Indicates whether the operation was successful.
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>
    /// Contains any error messages related to the operation.
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = default!;

    /// <summary>
    /// Contains any additional error details related to the operation.
    /// </summary>
    [JsonPropertyName("errors")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, string>? Errors { get; set; }
}

public class Response<T> : Response where T : class
{
    /// <summary>
    /// Contains the data returned from the operation.
    /// </summary>
    [JsonPropertyName("data")]
    public T? Data { get; set; }
}