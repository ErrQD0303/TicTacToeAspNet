using TicTacToeGame.Services.Bot;
using TicTacToeGame.Services.Interfaces;

namespace TicTacToeGame.Services;

public class BotService : IBotService
{
    public BotService()
    {
    }

    public Point GetBestMove(int[,] board, int player, BotType botType = BotType.Gomoku)
    {
        if (botType == BotType.TicTacToe)
        {
            return FirstEmptyCellBot.GetBestMove(board, player, true);
        }
        else if (botType == BotType.GomokuAI)
        {
            var result = new GomokuAI(board, player).GetMove();
            return new Point(result.Y, result.X); // Convert to Point (Y, X) format
        }

        throw new ArgumentException("Invalid bot type");
    }
}

public enum BotType
{
    TicTacToe,
    Gomoku,
    GomokuAI
}