using TicTacToeGame.Services.Bot;

namespace TicTacToeGame.Services.Interfaces;

public interface IBotService
{
    Point GetMove(int[,] board, int player, bool blockBothSides = false);
}