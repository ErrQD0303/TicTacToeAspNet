using TicTacToeGame.Models;

namespace TicTacToeGame.Hub;

public interface IGameHubClient
{
    Task ReceiveMove(string row, string column, string player, bool isViewer = false);
    Task ReceiveMatchFound(string opponent, string gameId, int row, int column, bool isBlockTwoSides = false);
    Task ReceiveExitMatch(string gameId, string message, bool isViewer = false);
    Task ReceiveMatchRestart(string newGameId, string message, int row, int column, bool isViewer = false);
    Task ReceiveUserAlreadyInAMatch(string gameId, string opponent);
    Task ReceiveSetNameSuccess(string id, string name);
    Task DisplayOnlineUsers(List<SimpleUser> users, string message);
    Task ReceiveMatchEnd(string gameId, string message, bool isViewer = false);
}