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

    public async Task Logout(string name)
    {
        var userId = Context.ConnectionId;
        var user = await Users.FirstOrDefaultUserWithNameAsync(name);
        if (user == null)
        {
            Logger.LogWarning("User not found: {UserId}", userId);
            throw new HubException("User not found.");
        }
        user.Name = string.Empty;
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
        if (currentUser == null)
        {
            Logger.LogWarning("User not found: {UserId}", Context.ConnectionId);
            throw new HubException("User not found.");
        }
        currentUser.Name = name;
        await Clients.Caller.ReceiveSetNameSuccess(currentUser.Id, currentUser.Name);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);

        var matches = CurrentMatches.Where(m => (m.Player1Id == Context.ConnectionId || m.Player2Id == Context.ConnectionId) && m.Result == MatchResult.Ongoing).ToList();
        var user = await Users.GetUserByIdAsync(Context.ConnectionId);
        if (user == null)
        {
            Logger.LogWarning("User not found: {UserId}", Context.ConnectionId);
            return;
        }

        await Users.RemoveAllAsync(user.Id);
        foreach (var match in matches)
        {
            match.Result = match.Player1Id == Context.ConnectionId ? MatchResult.Player2Win : MatchResult.Player1Win;
            Logger.LogInformation("User {0} and {1} exited match {MatchId}", match.Player1Id, match.Player2Id, match.Id);
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

        FindGameQueue.Enqueue(currentUser);

        if (FindGameQueue.Count < 2)
        {
            Logger.LogInformation("User {UserId} is waiting for a game", currentUser.Id);
            return;
        }

        if (!FindGameQueue.TryDequeue(out var player1) || !FindGameQueue.TryDequeue(out var player2))
        {
            Logger.LogWarning("Match creation failed due to queue issues.");
            // This should rarely happen, but safe to handle
            if (player1 != null) FindGameQueue.Enqueue(player1);
            return;
        }

        if (player1.Id == player2.Id)
        {
            Logger.LogWarning("Match creation failed: both players are the same.");
            FindGameQueue.Enqueue(player1);
            return;
        }

        var random = _random.Next(0, 2) == 0;

        var match = new SimpleMatch
        {
            Id = Guid.NewGuid().ToString(),
            Row = row,
            Column = column,
            Player1Id = random ? player1.Id : player2.Id,
            Player2Id = random ? player2.Id : player1.Id,
            IsPlayer1Turn = true,
            Result = MatchResult.Ongoing,
            Winner = string.Empty
        };

        var player1StillEligible = await IsUserStillAvailable(player1.Id);
        var player2StillEligible = await IsUserStillAvailable(player2.Id);

        if (!player1StillEligible || !player2StillEligible)
        {
            Logger.LogWarning("One of the players is no longer available: {Player1Id}, {Player2Id}", player1.Id, player2.Id);
            if (player1StillEligible) FindGameQueue.Enqueue(player1);
            if (player2StillEligible) FindGameQueue.Enqueue(player2);
            return;
        }

        CurrentMatches.Add(match);
        Logger.LogInformation("Match created: {MatchId} between {Player1Id} and {Player2Id}", match.Id, match.Player1Id, match.Player2Id);

        await Clients.Clients(match.Player1Id).ReceiveMatchFound(player1?.Name ?? "", match.Id.ToString(), match.Row, match.Column);
        await Clients.Clients(match.Player2Id).ReceiveMatchFound(player2?.Name ?? "", match.Id.ToString(), match.Row, match.Column);
        return;
    }

    private async Task<bool> IsUserStillAvailable(string userId)
    {
        var IsUserStillAvaiable = await Users.IsUserStillAvaiable(userId);
        var IsUserInMatch = CurrentMatches.Any(m => (m.Player1Id == userId || m.Player2Id == userId) && m.Result == MatchResult.Ongoing);
        return IsUserStillAvaiable && !IsUserInMatch;
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
