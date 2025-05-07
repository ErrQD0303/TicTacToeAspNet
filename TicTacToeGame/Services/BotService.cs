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

    public Point GetMove(string userId, Point opponentMove, int player)
    {
        var gomokuAi = GomokuAIs[userId];
        if (gomokuAi == null)
        {
            throw new InvalidOperationException("GomokuAI is not initialized. Call InitializeBoard first.");
        }

        if (opponentMove.r >= 0 && opponentMove.r < CurrentRow && opponentMove.c >= 0 && opponentMove.c < CurrentColumn)
        {
            // Valid move
            gomokuAi.UpdateBoard(opponentMove, 3 - player);
        }

        var playerMove = gomokuAi.GetMove() ?? throw new InvalidOperationException("No valid move found.");
        gomokuAi.UpdateBoard(playerMove, player);
        return playerMove;
    }

    public void InitializeBoard(string userId, int[,] board, int player, bool blockTwoSides = false)
    {
        var row = board.GetLength(0);
        var column = board.GetLength(1);
        CurrentRow = row;
        CurrentColumn = column;
        GomokuAIs[userId] = new GomokuAI(board, player, blockTwoSides);
    }
}

public enum BotType
{
    TicTacToe,
    Gomoku,
    GomokuAI
}