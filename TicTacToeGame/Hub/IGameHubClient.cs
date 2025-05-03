using TicTacToeGame.Models;

namespace TicTacToeGame.Hub;

public interface IGameHubClient
{
    Task ReceiveMove(string row, string column, string player, bool isViewer = false);
    Task ReceiveMatchFound(string opponent, SimpleMatch match);
    Task ReceiveExitMatch(string gameId, string message, bool isViewer = false);
    Task ReceiveMatchRestart(string newGameId, string message, SimpleMatch match, bool isViewer = false);
    Task ReceiveUserAlreadyInAMatch(string gameId, string opponent);
    Task ReceiveSetNameSuccess(string id, string name);
    Task DisplayOnlineUsers(List<SimpleUser> users, string message);
    Task ReceiveMatchEnd(string gameId, string message, bool isViewer = false);
}