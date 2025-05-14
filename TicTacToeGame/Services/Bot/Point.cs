namespace TicTacToeGame.Services.Bot;

public class Point(int x, int y) : IEquatable<Point>
{
    public int R { get; set; } = x;
    public int C { get; set; } = y;

    public bool Equals(Point? other)
    {
        if (other is null)
        {
            return false;
        }

        return R == other.R && C == other.C;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Point);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(R, C);
    }

    public override string ToString()
    {
        return $"({R}, {C})";
    }
}