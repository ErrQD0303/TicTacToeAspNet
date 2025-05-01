
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TicTacToeGame.Helpers.Enum;
using TicTacToeGame.Helpers.Mappers;
using TicTacToeGame.Models;
using TicTacToeGame.Services.Interfaces;

namespace TicTacToeGame.Hub;

[Authorize]
public class GameHub : Hub<IGameHubClient>
{
    public ILogger<GameHub> Logger { get; }
    public static List<TicTacToeMatch> CurrentMatches { get; } = [];
    public IUserService UserService { get; }
    public ITicTacToeMatchService TicTacToeMatchService { get; }
    private static string AllChatGroupId { get; } = "AllChat";
    private static readonly Random _random = new();
    private static ConcurrentDictionary<string, List<string>> UserConnections { get; } = new();

    public GameHub(IUserService userService, ITicTacToeMatchService ticTacToeMatchService, ILogger<GameHub> logger)
    {
        Logger = logger;
        UserService = userService;
        this.TicTacToeMatchService = ticTacToeMatchService;
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();

        var user = await UserService.GetUserByIdAsync(Context.UserIdentifier!);
        if (user == null)
        {
            Logger.LogWarning("User not found: {UserId}", Context.UserIdentifier);
            throw new HubException("User not found");
        }
        Logger.LogInformation("User connected: {UserId}", user.Id);
        if (!UserConnections.TryGetValue(user.Id, out var connections))
        {
            connections = [];
            UserConnections.TryAdd(user.Id, connections);
        }

        connections.Add(Context.ConnectionId);
        Logger.LogInformation("User {UserId} has {ConnectionCount} connections",
        user.Id, connections.Count);
        var existedMatch = FindExistedMatch(user.Id);
        if (!string.IsNullOrEmpty(existedMatch))
        {
            Logger.LogInformation("User {UserId} is already in a match: {MatchId}", user.Id, existedMatch);
            await Clients.Caller.ReceiveUserAlreadyInAMatch(existedMatch, user.Id);
        }
        else
        {
            Logger.LogInformation("User {UserId} is not in a match", user.Id);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);

        var user = await UserService.GetUserByIdAsync(Context.UserIdentifier!);
        if (user == null)
        {
            Logger.LogWarning("User not found: {UserId}", Context.UserIdentifier);
            return;
        }

        Logger.LogInformation("User disconnected: {UserId}", user.Id);
        if (UserConnections.TryGetValue(user.Id, out var connections))
        {
            connections.Remove(Context.ConnectionId);
            Logger.LogInformation("User {UserId} has {ConnectionCount} connections", user.Id, connections.Count);
            if (connections.Count == 0)
            {
                UserConnections.TryRemove(user.Id, out _);
                Logger.LogInformation("User {UserId} removed from connections", user.Id);
            }
        }
    }

    public async Task FindGame(int row, int column)
    {
        var you = (await UserService.GetUserByIdAsync(Context.UserIdentifier!))?.ToAppUser();
        if (you == null)
        {
            Logger.LogWarning("User not found: {UserId}", Context.UserIdentifier);
            throw new HubException("User not found");
        }

        var existedMatch = FindExistedMatch(Context.UserIdentifier!);
        if (!string.IsNullOrEmpty(existedMatch))
        {
            Logger.LogWarning("User {UserId} is already in a match: {MatchId}", you.Id, existedMatch);
            var userConnections = UserConnections[you.Id];
            await Clients.Clients(userConnections).ReceiveUserAlreadyInAMatch(existedMatch, you.Id);
            return;
        }

        Logger.LogInformation("User {UserId} is looking for a game", you.Id);

        var findingMatch = CurrentMatches.FirstOrDefault(m => m.Player2 == null);
        if (findingMatch != null)
        {
            var random = _random.Next(0, 2);
            if (random == 0)
            {
                findingMatch.Player2Id = findingMatch.Player1Id;
                findingMatch.Player2 = findingMatch.Player1;
                findingMatch.Player1Id = you.Id;
                findingMatch.Player1 = you;
            }
            else
            {
                findingMatch.Player2Id = you.Id;
                findingMatch.Player2 = you;
            }
            findingMatch.IsPlayer1Turn = true;
            var player1Connetions = UserConnections[findingMatch.Player1!.Id] ?? [];
            var player2Connections = UserConnections[findingMatch.Player2!.Id] ?? [];
            var connections = player1Connetions.Concat(player2Connections).ToList();
            await Clients.Clients(connections).ReceiveMatchFound(findingMatch.Player1!.UserName, findingMatch.Id.ToString(), findingMatch.Row, findingMatch.Column);
            return;
        }

        var newMatch = new TicTacToeMatch(row, column)
        {
            Id = Guid.NewGuid().ToString(),
            Player1Id = you.Id,
            Player1 = you,
            TicTacToeMatchHistory = new TicTacToeMatchHistory
            {
                Result = MatchResult.Ongoing
            }
        };

        CurrentMatches.Add(newMatch);
        return;
    }

    public async Task EndMatch(string matchId, string winner, string[][] board)
    {
        var userId = GetUserId();
        var match = CurrentMatches.FirstOrDefault(m => m.Id.ToString() == matchId);
        if (match == null)
        {
            Logger.LogWarning("Match not found: {MatchId}", matchId);
            throw new HubException("Match not found.");
        }

        if (match.TicTacToeMatchHistory.Result != MatchResult.Ongoing)
        {
            Logger.LogWarning("Match is not ongoing: {MatchId}", matchId);
            throw new HubException("Match is not ongoing.");
        }

        match.TicTacToeMatchHistory.Result = winner switch
        {
            "X" => MatchResult.Player1Win,
            "4" => MatchResult.Player2Win,
            _ => MatchResult.Draw
        };

        match.Board = new CellState[match.Row, match.Column];
        for (int i = 0; i < match.Row; i++)
        {
            for (int j = 0; j < match.Column; j++)
            {
                match.Board[i, j] = board[i][j] switch
                {
                    "X" => CellState.X,
                    "O" => CellState.O,
                    _ => CellState.Empty
                };
            }
        }
        await TicTacToeMatchService.AddAsync(match.ToCreateTicTacToeMatchDto());
    }

    public async Task ExitMatch(string matchId)
    {
        var userId = GetUserId();
        var match = CurrentMatches.FirstOrDefault(m => m.Id.ToString() == matchId && (m.Player1?.Id == userId || m.Player2?.Id == userId));
        if (match == null)
        {
            Logger.LogWarning("Match not found: {MatchId}", matchId);
            return;
        }

        if (match.TicTacToeMatchHistory.Result != MatchResult.Ongoing)
        {
            Logger.LogWarning("Match is not ongoing: {MatchId}", matchId);
            return;
        }

        match.TicTacToeMatchHistory.Result = match.Player1?.Id == userId ? MatchResult.Player2Win : MatchResult.Player1Win;
        await TicTacToeMatchService.AddAsync(match.ToCreateTicTacToeMatchDto());

        Logger.LogInformation("User {0} and {1} exited match {MatchId}", userId, match.Player2Id, matchId);
        var player1Connections = UserConnections[match.Player1Id ?? ""] ?? [];
        var player2Connections = UserConnections[match.Player2Id ?? ""] ?? [];
        var connections = player1Connections.Concat(player2Connections).ToList();

        await Clients.Clients(connections).ReceiveExitMatch(match.Player1!.Id, $"Player {userId} exited the match");
    }

    public async Task Restart(string matchId, int row, int column)
    {
        var userId = GetUserId();
        var match = CurrentMatches.LastOrDefault(m => m.Id.ToString() == matchId);
        if (match == null)
        {
            Logger.LogWarning("Match not found: {UserId}", userId);
            throw new HubException("Match not found.");
        }

        if (match.TicTacToeMatchHistory.Result == MatchResult.Ongoing)
        {
            match.TicTacToeMatchHistory.Result = MatchResult.Draw;
            await TicTacToeMatchService.AddAsync(match.ToCreateTicTacToeMatchDto());
        }

        var player1Connections = UserConnections[match.Player1Id ?? ""] ?? [];
        var player2Connections = UserConnections[match.Player2Id ?? ""] ?? [];
        var connections = player1Connections.Concat(player2Connections).ToList();

        var newMatch = new TicTacToeMatch(match.Row, match.Column)
        {
            Id = Guid.NewGuid().ToString(),
            Row = row,
            Column = column,
            Player1Id = match.Player2Id,
            Player1 = match.Player2,
            Player2Id = match.Player1Id,
            Player2 = match.Player1,
            IsPlayer1Turn = true,
            TicTacToeMatchHistory = new TicTacToeMatchHistory
            {
                Result = MatchResult.Ongoing
            }
        };

        CurrentMatches.Add(newMatch);

        await Clients.Clients(connections).ReceiveMatchRestart(newMatch.Id, $"Player {userId} restarted the match", newMatch.Row, newMatch.Column);
    }

    public async Task SendMove(string row, string column, string matchId)
    {
        var userId = GetUserId();
        var connectionId = Context.ConnectionId;

        var match = GetMatchById(matchId);

        if ((match.Player1?.Id == userId && match.IsPlayer1Turn == false) || (match.Player2?.Id == userId && match.IsPlayer1Turn == true))
        {
            Logger.LogWarning("It's not your turn: {UserId}", userId);
            throw new HubException("It's not your turn.");
        }
        match.IsPlayer1Turn = !match.IsPlayer1Turn;

        var player1Connections = UserConnections[match.Player1Id ?? ""] ?? [];
        var player2Connections = UserConnections[match.Player2Id ?? ""] ?? [];
        var connections = player1Connections.Concat(player2Connections).ToList();

        await Clients.Clients(connections).ReceiveMove(row, column, match.Player1Id == userId ? CellState.X.ToString() : CellState.O.ToString());
    }

    public string GetMark(string matchId)
    {
        var match = GetMatchById(matchId);
        return match.Player1Id == Context.UserIdentifier ? CellState.X.ToString() : CellState.O.ToString();
    }

    private static bool IsUserInMatch(string userId, string matchId)
    {
        return CurrentMatches.Any(m => m.Id.ToString() == matchId && (m.Player1?.Id == userId || m.Player2?.Id == userId));
    }

    private TicTacToeMatch GetMatchById(string matchId)
    {
        var userId = GetUserId();

        var match = CurrentMatches.FirstOrDefault(m => m.Id.ToString() == matchId);
        if (match == null)
        {
            Logger.LogWarning("Match not found: {MatchId}", matchId);
            throw new HubException("Match not found.");
        }

        if (!IsUserInMatch(userId, matchId))
        {
            throw new HubException("You are not a player in this match.");
        }

        if (match.TicTacToeMatchHistory.Result != MatchResult.Ongoing)
        {
            throw new HubException("Match is not ongoing.");
        }

        return match;
    }

    private string GetUserId()
    {
        var userId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(userId))
        {
            throw new HubException("User not found.");
        }
        return userId;
    }

    private static string FindExistedMatch(string userId)
    {
        var match = CurrentMatches.LastOrDefault(m => (m.Player1?.Id == userId || m.Player2?.Id == userId) && m.TicTacToeMatchHistory.Result == MatchResult.Ongoing);
        if (match == null)
        {
            return string.Empty;
        }
        return match.Id.ToString();
    }
}
