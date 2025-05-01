namespace TicTacToeGame.Hub;

public interface IGameHubClient
{
    Task ReceiveMove(string row, string column, string player);
    Task ReceiveMatchFound(string opponent, string gameId, int row, int column);
    Task ReceiveExitMatch(string gameId, string message);
    Task ReceiveMatchRestart(string newGameId, string message, int row, int column);
    Task ReceiveUserAlreadyInAMatch(string gameId, string opponent);
}