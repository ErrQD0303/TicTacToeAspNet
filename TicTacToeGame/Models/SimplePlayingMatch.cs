using Newtonsoft.Json;

namespace TicTacToeGame.Models;

public class SimplePlayingMatch
{
    public string Id { get; set; } = default!;
    public SimpleUser Player1 { get; set; } = default!;
    public SimpleUser Player2 { get; set; } = default!;
    public bool?[][] Board { get; set; } = default!;
    public int Row { get; set; } = default!;
    public int Column { get; set; } = default!;
    public bool IsPlayer1Turn { get; set; } = default!;
    public bool IsBlockTwoSides { get; set; } = default!;
    public SimpleMove PreviousMove { get; set; } = new SimpleMove
    {
        Row = -1,
        Col = -1
    };
    public bool IsBotGame { get; set; } = false;
}