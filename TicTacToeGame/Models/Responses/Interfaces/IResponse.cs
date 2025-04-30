using System.Text.Json.Serialization;

namespace TicTacToeGame.Models.Responses.Interfaces;

public interface IResponse
{
    /// <summary>
    /// Indicates whether the operation was successful.
    /// </summary>
    [JsonPropertyName("success")]
    bool Success { get; set; }

    /// <summary>
    /// Contains any error messages related to the operation.
    /// </summary>
    [JsonPropertyName("message")]
    string Message { get; set; }

    /// <summary>
    /// Contains any additional error details related to the operation.
    /// </summary>
    [JsonPropertyName("errors")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    Dictionary<string, string>? Errors { get; set; }
}

public interface IResponse<T> : IResponse where T : class
{
    /// <summary>
    /// Contains the data returned from the operation.
    /// </summary>
    [JsonPropertyName("data")]
    T Data { get; set; }
}