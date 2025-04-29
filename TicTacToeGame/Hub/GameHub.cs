
using Microsoft.AspNetCore.SignalR;
using TicTacToeGame.Models;

namespace TicTacToeGame.Hub;

public class GameHub : Microsoft.AspNetCore.SignalR.Hub<IGameHubClient>
{
    public static TicTacToeMatch CurrentMatch = new();

    public override async Task OnConnectedAsync()
    {
        string connectionId = Context.ConnectionId;
        if (string.IsNullOrEmpty(CurrentMatch.Player1))
        {
            CurrentMatch.Player1 = connectionId;
        }
        else if (string.IsNullOrEmpty(CurrentMatch.Player2))
        {
            CurrentMatch.Player2 = connectionId;
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (CurrentMatch.Player1 == Context.ConnectionId)
        {
            CurrentMatch.Player1 = string.Empty;
        }
        else if (CurrentMatch.Player2 == Context.ConnectionId)
        {
            CurrentMatch.Player2 = string.Empty;
        }
        CurrentMatch.IsPlayer1Turn = true;
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMove(string row, string column, string player)
    {
        string connectionId = Context.ConnectionId;
        if (connectionId != CurrentMatch.Player1 && connectionId != CurrentMatch.Player2)
        {
            throw new HubException("You are not a player in this match.");
        }
        if ((connectionId == CurrentMatch.Player1 && !CurrentMatch.IsPlayer1Turn) ||
         (connectionId == CurrentMatch.Player2 && CurrentMatch.IsPlayer1Turn))
        {
            throw new HubException("It's not your turn.");
        }
        CurrentMatch.IsPlayer1Turn = !CurrentMatch.IsPlayer1Turn;
        await Clients.All.ReceiveMove(row, column, player);
    }

    public string GetMark()
    {
        string connectionId = Context.ConnectionId;
        if (connectionId != CurrentMatch.Player1 && connectionId != CurrentMatch.Player2)
        {
            throw new HubException("You are not a player in this match.");
        }

        return connectionId == CurrentMatch.Player1 ? TicTacToeMatch.Player1Mark : TicTacToeMatch.Player2Mark;
    }
}