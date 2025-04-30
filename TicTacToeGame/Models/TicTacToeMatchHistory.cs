using TicTacToeGame.Helpers.Enum;

namespace TicTacToeGame.Models;

public class TicTacToeMatchHistory
{
    public virtual string TicTacToeMatchId { get; set; } = default!;
    public virtual MatchResult Result { get; set; } = MatchResult.Ongoing;
    public virtual DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual TicTacToeMatch TicTacToeMatch { get; set; } = default!;
}