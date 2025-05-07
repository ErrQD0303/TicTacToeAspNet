using TicTacToeGame.Services.Bot;

namespace TicTacToeGame.Services.Interfaces;

public interface IBotService
{
    void InitializeBoard(string userId, int[,] board, int player, bool blockTwoSides = false);
    Point GetMove(string userId, Point move, int player);
}