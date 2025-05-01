using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicTacToeGame.Helpers.Enum;

namespace TicTacToeGame.Models.Dtos.TicTacToeMatches;

public class CreateTicTacToeMatchDto
{
    public string Id { get; set; } = default!;
    public int Row;
    public int Column;
    public CellState[,] Board { get; set; } = default!;
    public string? Player1Id { get; set; } = default!;
    public string? Player2Id { get; set; } = default!;
    public bool IsPlayer1Turn { get; set; } = true;
    public MatchResult Result { get; set; } = MatchResult.Ongoing;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
