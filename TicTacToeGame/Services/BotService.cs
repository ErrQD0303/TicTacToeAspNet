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
        return botType switch
        {
            BotType.TicTacToe => FirstEmptyCellBot.GetBestMove(board, player),
            BotType.Gomoku => GomokuBot.GetBestMove(board, player),
            _ => throw new ArgumentOutOfRangeException(nameof(botType), botType, null)
        };
    }
}

public enum BotType
{
    TicTacToe,
    Gomoku
}