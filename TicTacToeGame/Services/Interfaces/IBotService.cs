using TicTacToeGame.Services.Bot;

namespace TicTacToeGame.Services.Interfaces;

public interface IBotService
{
    Point GetBestMove(int[,] board, int player, BotType botType = BotType.Gomoku);
}