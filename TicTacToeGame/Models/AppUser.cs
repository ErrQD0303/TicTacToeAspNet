namespace TicTacToeGame.Models;

public class AppUser
{
    public virtual string Id { get; set; } = default!;
    public virtual string UserName { get; set; } = default!;
    public virtual string Name { get; set; } = default!;
    public virtual string Email { get; set; } = default!;
    public virtual string HashedPassword { get; set; } = default!;

    // Navigation properties
    public virtual ICollection<TicTacToeMatch> MatchesAsPlayer1 { get; set; } = [];
    public virtual ICollection<TicTacToeMatch> MatchesAsPlayer2 { get; set; } = [];
    public virtual ICollection<TicTacToeMatch> TicTacToeMatches
    {
        get
        {
            return [.. MatchesAsPlayer1, .. MatchesAsPlayer2];
        }
    }
};
