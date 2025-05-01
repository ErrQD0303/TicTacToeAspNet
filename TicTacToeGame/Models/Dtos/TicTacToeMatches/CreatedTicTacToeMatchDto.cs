using TicTacToeGame.Helpers.Enum;

namespace TicTacToeGame.Models.Dtos.TicTacToeMatches;

public class CreatedTicTacToeMatchDto
{
    public string Id { get; set; } = default!;
    public int Row;
    public int Column;
    public CellState[,] Board { get; set; } = default!;
    public string? Player1Id { get; set; } = default!;
    public string? Player2Id { get; set; } = default!;
    public bool IsPlayer1Turn { get; set; } = true;
    public CreatedTicTacToeMatchHistoryDto TicTacToeMatchHistory { get; set; } = default!;
}

