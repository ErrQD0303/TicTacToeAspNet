using TicTacToeGame.Services.Bot;
using TicTacToeGame.Services.Interfaces;

namespace TicTacToeGame.Services;

public class BotService : IBotService
{
    private static readonly Dictionary<string, GomokuAI> GomokuAIs = [];
    private static int CurrentRow;
    private static int CurrentColumn;
    public BotService()
    {
    }

    public Point GetMove(int[,] board, int player, bool blockBothSides = false)
    {
        return new TraditionalGomokuAI(board, player, blockBothSides).GetMove();
    }
}

public enum BotType
{
    TicTacToe,
    Gomoku,
    GomokuAI
}