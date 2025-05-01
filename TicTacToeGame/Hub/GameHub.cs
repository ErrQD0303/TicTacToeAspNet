using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using TicTacToeGame.Helpers.Enum;
using TicTacToeGame.Helpers.Mappers;
using TicTacToeGame.Models;
using TicTacToeGame.Services.Interfaces;

namespace TicTacToeGame.Hub;

// [Authorize]
public class GameHub : Hub<IGameHubClient>
{
    public ILogger<GameHub> Logger { get; }
    public static List<SimpleMatch> CurrentMatches { get; } = [];
    public static ConcurrentQueue<SimpleUser> FindGameQueue { get; } = new();
    public static ISimpleUserService Users { get; private set; } = default!;
    private static readonly Random _random = new();

    public GameHub(ILogger<GameHub> logger, ISimpleUserService userService)
    {
        Users = userService;
        Logger = logger;
    }
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        Users = Users ?? throw new InvalidOperationException("User service is not initialized.");
        await Users.LoginAsync(Context.ConnectionId);
    }

    public async Task SetName(string name)
    {
        var existedUser = await Users.FirstOrDefaultUserWithNameAsync(name);
        if (existedUser != null)
        {
            Logger.LogWarning("User found: {UserId}", Context.ConnectionId);
            throw new HubException($"User with this name {name} already exists.");
        }

        var currentUser = await Users.GetUserByIdAsync(Context.ConnectionId);
        currentUser.Name = name;
        await Clients.Caller.ReceiveSetNameSuccess(currentUser.Id, currentUser.Name);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);

        await Users.RemoveAllAsync(Context.ConnectionId);
        var matches = CurrentMatches.Where(m => (m.Player1Id == Context.ConnectionId || m.Player2Id == Context.ConnectionId) && m.Result == MatchResult.Ongoing).ToList();
        var user = await Users.GetUserByIdAsync(Context.ConnectionId);
        if (user == null)
        {
            Logger.LogWarning("User not found: {UserId}", Context.ConnectionId);
            return;
        }
        foreach (var match in matches)
        {
            await Clients.Clients([match.Player1Id, match.Player2Id]).ReceiveExitMatch(match.Player1Id, $"Player {user.Name} exited the match");
        }
    }

    public async Task FindGame(int row, int column)
    {
        var currentUser = await Users.GetUserByIdAsync(Context.ConnectionId);
        if (currentUser == null)
        {
            Logger.LogWarning("User not found: {UserId}", Context.ConnectionId);
            throw new HubException("User not found.");
        }

        var existedMatch = FindExistedMatch(Context.ConnectionId);
        if (!string.IsNullOrEmpty(existedMatch))
        {
            Logger.LogWarning("User {UserId} is already in a match: {MatchId}", currentUser.Id, existedMatch);
            await Clients.Caller.ReceiveUserAlreadyInAMatch(existedMatch, currentUser.Id);
            return;
        }

        Logger.LogInformation("User {UserId} is looking for a game", currentUser.Id);

        var findingMatch = CurrentMatches.FirstOrDefault(m => string.IsNullOrEmpty(m.Player2Id));
        if (findingMatch != null)
        {
            var random = _random.Next(0, 2);
            if (random == 0)
            {
                findingMatch.Player2Id = findingMatch.Player1Id;
                findingMatch.Player1Id = currentUser.Id;
            }
            else
            {
                findingMatch.Player2Id = currentUser.Id;
            }
            findingMatch.IsPlayer1Turn = true;
            var player1 = await Users.GetUserByIdAsync(findingMatch.Player1Id);
            var player2 = await Users.GetUserByIdAsync(findingMatch.Player2Id);
            await Clients.Clients(findingMatch.Player1Id).ReceiveMatchFound(player1?.Name ?? "", findingMatch.Id.ToString(), findingMatch.Row, findingMatch.Column);
            await Clients.Clients(findingMatch.Player2Id).ReceiveMatchFound(player2?.Name ?? "", findingMatch.Id.ToString(), findingMatch.Row, findingMatch.Column);
            return;
        }

        var newMatch = new SimpleMatch
        {
            Id = Guid.NewGuid().ToString(),
            Row = row,
            Column = column,
            Player1Id = currentUser.Id,
            IsPlayer1Turn = true,
            Result = MatchResult.Ongoing,
            Winner = string.Empty,
        };

        CurrentMatches.Add(newMatch);
        return;
    }

    public Task EndMatch(string matchId, string winner, string[][] board)
    {
        var userId = GetUserId();
        var match = CurrentMatches.FirstOrDefault(m => m.Id.ToString() == matchId);
        if (match == null)
        {
            Logger.LogWarning("Match not found: {MatchId}", matchId);
            throw new HubException("Match not found.");
        }

        if (match.Result != MatchResult.Ongoing)
        {
            Logger.LogWarning("Match is not ongoing: {MatchId}", matchId);
            throw new HubException("Match is not ongoing.");
        }

        match.Result = winner switch
        {
            "X" => MatchResult.Player1Win,
            "4" => MatchResult.Player2Win,
            _ => MatchResult.Draw
        };

        return Task.CompletedTask;
    }

    public async Task ExitMatch(string matchId)
    {
        var userId = GetUserId();
        var match = CurrentMatches.FirstOrDefault(m => m.Id.ToString() == matchId && (m.Player1Id == userId || m.Player2Id == userId));
        if (match == null)
        {
            Logger.LogWarning("Match not found: {MatchId}", matchId);
            return;
        }

        if (match.Result != MatchResult.Ongoing)
        {
            Logger.LogWarning("Match is not ongoing: {MatchId}", matchId);
            return;
        }

        match.Result = match.Player1Id == userId ? MatchResult.Player2Win : MatchResult.Player1Win;

        Logger.LogInformation("User {0} and {1} exited match {MatchId}", userId, match.Player2Id, matchId);
        var currentUser = await Users.GetUserByIdAsync(userId);

        await Clients.Clients([match.Player1Id, match.Player2Id]).ReceiveExitMatch(match.Player1Id, $"Player {currentUser?.Name ?? ""} exited the match");
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

        if (match.Result == MatchResult.Ongoing)
        {
            match.Result = MatchResult.Draw;
        }

        var newMatch = new SimpleMatch
        {
            Id = Guid.NewGuid().ToString(),
            Row = row,
            Column = column,
            Player1Id = match.Player2Id,
            Player2Id = match.Player1Id,
            IsPlayer1Turn = true,
            Result = MatchResult.Ongoing,
            Winner = string.Empty,
        };

        CurrentMatches.Add(newMatch);

        var currentUser = await Users.GetUserByIdAsync(userId);
        await Clients.Clients([newMatch.Player1Id, newMatch.Player2Id]).ReceiveMatchRestart(newMatch.Id, $"Player {currentUser?.Name ?? ""} restarted the match", newMatch.Row, newMatch.Column);
    }

    public async Task SendMove(string row, string column, string matchId)
    {
        var userId = GetUserId();

        var match = GetMatchById(matchId);

        if ((match.Player1Id == userId && match.IsPlayer1Turn == false) || (match.Player2Id == userId && match.IsPlayer1Turn == true))
        {
            Logger.LogWarning("It's not your turn: {UserId}", userId);
            throw new HubException("It's not your turn.");
        }
        match.IsPlayer1Turn = !match.IsPlayer1Turn;

        await Clients.Clients([match.Player1Id, match.Player2Id]).ReceiveMove(row, column, match.Player1Id == userId ? CellState.X.ToString() : CellState.O.ToString());
    }

    public string GetMark(string matchId)
    {
        var match = GetMatchById(matchId);
        return match.Player1Id == Context.ConnectionId ? CellState.X.ToString() : CellState.O.ToString();
    }

    private static bool IsUserInMatch(string userId, string matchId)
    {
        return CurrentMatches.Any(m => m.Id.ToString() == matchId && (m.Player1Id == userId || m.Player2Id == userId));
    }

    private SimpleMatch GetMatchById(string matchId)
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

        if (match.Result != MatchResult.Ongoing)
        {
            throw new HubException("Match is not ongoing.");
        }

        return match;
    }

    private string GetUserId()
    {
        var userId = Context.ConnectionId;
        if (string.IsNullOrEmpty(userId))
        {
            throw new HubException("User not found.");
        }
        return userId;
    }

    private static string FindExistedMatch(string userId)
    {
        var match = CurrentMatches.LastOrDefault(m => (m.Player1Id == userId || m.Player2Id == userId) && m.Result == MatchResult.Ongoing);
        if (match == null)
        {
            return string.Empty;
        }
        return match.Id.ToString();
    }
}
