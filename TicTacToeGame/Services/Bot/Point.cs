namespace TicTacToeGame.Services.Bot;

public class Point(int x, int y)
{
    public int r { get; set; } = x;
    public int c { get; set; } = y;

    public override string ToString()
    {
        return $"({r}, {c})";
    }
}