namespace TicTacToeGame.Hub;

public interface IGameHubClient
{
    Task ReceiveMove(string row, string column, string player);
}