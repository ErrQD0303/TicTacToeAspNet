using TicTacToeGame.Helpers.Enum;

namespace TicTacToeGame.Models;

public class SimpleMatch
{
    public string Id { get; set; } = default!;
    public string Player1Id { get; set; } = default!;
    public string Player2Id { get; set; } = default!;
    public bool IsPlayer1Turn { get; set; } = default!;
    public string Winner { get; set; } = default!;
    public MatchResult Result { get; set; } = default!;
    public int Row { get; set; } = default!;
    public int Column { get; set; } = default!;
}