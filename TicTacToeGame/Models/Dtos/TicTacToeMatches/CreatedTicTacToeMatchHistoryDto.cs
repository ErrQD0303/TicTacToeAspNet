using TicTacToeGame.Helpers.Enum;

namespace TicTacToeGame.Models.Dtos.TicTacToeMatches;

public class CreatedTicTacToeMatchHistoryDto
{
    public virtual MatchResult Result { get; set; } = MatchResult.Ongoing;
    public virtual DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}