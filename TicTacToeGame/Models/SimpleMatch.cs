using Newtonsoft.Json;
using TicTacToeGame.Helpers.Enum;

namespace TicTacToeGame.Models;

public class SimpleMatch
{
    public string Id { get; set; } = default!;
    public string Player1Id { get; set; } = default!;
    public string Player1Name { get; set; } = default!;
    public string Player2Id { get; set; } = default!;
    public string Player2Name { get; set; } = default!;
    public bool IsPlayer1Turn { get; set; } = default!;
    public SimpleMove PreviousMove { get; set; } = default!;
    public string WinnerId { get; set; } = default!;
    public bool?[][] Board { get; set; } = default!;
    public MatchResult Result { get; set; } = default!;
    public int Row { get; set; } = default!;
    public int Column { get; set; } = default!;
    public bool IsBlockTwoSides { get; set; } = default!;
    public List<SimpleUser> Viewers { get; set; } = default!;
}