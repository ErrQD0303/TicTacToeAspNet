namespace TicTacToeGame.Services.Bot;

public class PlayerPattern
{
    public BlockType BlockType { get; set; }
    public int ConsecutiveCells { get; set; }

    public PlayerPattern(BlockType blockType, int consecutiveCells)
    {
        BlockType = blockType;
        ConsecutiveCells = consecutiveCells;
    }
}