using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using TicTacToeGame.Helpers;
using TicTacToeGame.Helpers.Enum;
using TicTacToeGame.Helpers.Mappers;
using TicTacToeGame.Models;
using TicTacToeGame.Services;
using TicTacToeGame.Services.Bot;
using TicTacToeGame.Services.Interfaces;

namespace TicTacToeGame.Hub;

// [Authorize]
public class GameHub : Hub<IGameHubClient>
{
    public ILogger<GameHub> Logger { get; }
    public static List<SimpleMatch> CurrentMatches { get; } = [];
    public static ConcurrentQueue<SimpleUser> FindGameQueue { get; } = new();
    public static ISimpleUserService Users { get; private set; } = default!;
    public static IBotService BotService { get; private set; } = default!;
    private static readonly Random _random = new();

    public GameHub(ILogger<GameHub> logger, ISimpleUserService userService, IBotService botService)
    {
        Logger = logger;
        Users = userService;
        BotService = botService;
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
        await Clients.All.DisplayOnlineUsers(await Users.GetAllValidUsers(), $"User {user.Name} disconnected");
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
        await Clients.All.DisplayOnlineUsers(await Users.GetAllValidUsers(), $"User {currentUser.Name} connected");
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
            await Clients.Clients(match.Viewers.Select(v => v.Id)).ReceiveExitMatch(match.Player1Id, $"Player {user.Name} exited the match");
        }
        await Clients.All.DisplayOnlineUsers(await Users.GetAllValidUsers(), $"User {user.Name} disconnected");
    }

    public async Task FindGame(int row, int column, bool isBlockTwoSides = false)
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
            Player1Name = random ? player1.Name : player2.Name,
            Player2Id = random ? player2.Id : player1.Id,
            Player2Name = random ? player2.Name : player1.Name,
            Board = [.. Enumerable.Range(0, row).Select(_ => new bool?[column])],
            Viewers = new List<SimpleUser>(),
            IsPlayer1Turn = true,
            Result = MatchResult.Ongoing,
            IsBlockTwoSides = isBlockTwoSides,
            WinnerId = string.Empty
        };

        var player1StillEligible = await IsUserStillAvailable(player1.Id);
        var player2StillEligible = await IsUserStillAvailable(player2.Id);

        if (!player1StillEligible || !player2StillEligible)
        {
            Logger.LogWarning("One of the players is no longer available: {Player1Id}, {PRlayer2Id}", player1.Id, player2.Id);
            if (player1StillEligible) FindGameQueue.Enqueue(player1);
            if (player2StillEligible) FindGameQueue.Enqueue(player2);
            return;
        }

        CurrentMatches.Add(match);
        Logger.LogInformation("Match created: {MatchId} between {Player1Id} and {Player2Id}", match.Id, match.Player1Id, match.Player2Id);

        await Clients.Clients(match.Player1Id).ReceiveMatchFound(player2?.Name ?? "", match);
        await Clients.Clients(match.Player2Id).ReceiveMatchFound(player1?.Name ?? "", match);
        return;
    }

    private async Task<bool> IsUserStillAvailable(string userId)
    {
        var IsUserStillAvaiable = await Users.IsUserStillAvaiable(userId);
        var IsUserInMatch = CurrentMatches.Any(m => (m.Player1Id == userId || m.Player2Id == userId) && m.Result == MatchResult.Ongoing);
        return IsUserStillAvaiable && !IsUserInMatch;
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

        if (match.Result != MatchResult.Ongoing)
        {
            Logger.LogWarning("Match is not ongoing: {MatchId}", matchId);
            throw new HubException("Match is not ongoing.");
        }

        match.Result = winner switch
        {
            "X" => MatchResult.Player1Win,
            "O" => MatchResult.Player2Win,
            _ => MatchResult.Draw
        };

        match.WinnerId = winner switch
        {
            "X" => match.Player1Id,
            "O" => match.Player2Id,
            _ => string.Empty
        };

        string message;

        if (winner == "X" || winner == "O")
        {
            message = $"Match ended: {match.Player1Name} wins!";
            Logger.LogInformation("Match ended: {MatchId} - {Winner}", match.Id, winner);
        }
        else
        {
            message = "Match ended in a draw";
            Logger.LogInformation("Match ended in a draw: {MatchId}", match.Id);
        }

        await Clients.Clients(match.Viewers.Select(v => v.Id)).ReceiveMatchEnd(match.Id, message, true);
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

        await Clients.Clients([match.Player1Id, match.Player2Id]).ReceiveExitMatch(match.Player1Id, $"Player {currentUser?.Name ?? ""} exited the match", false);
        await Clients.Clients(match.Viewers.Select(v => v.Id)).ReceiveExitMatch(match.Player1Id, $"Player {currentUser?.Name ?? ""} exited the match", true);
    }

    public async Task Restart(string matchId, int row, int column, bool isBlockTwoSides = false)
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
            Player1Name = match.Player2Name,
            Player2Id = match.Player1Id,
            Player2Name = match.Player1Name,
            Board = [.. Enumerable.Range(0, row).Select(_ => new bool?[column])],
            Viewers = match.Viewers,
            IsPlayer1Turn = true,
            Result = MatchResult.Ongoing,
            IsBlockTwoSides = isBlockTwoSides,
            WinnerId = string.Empty,
            IsBotGame = match.IsBotGame,
            PreviousMove = new SimpleMove
            {
                Row = -1,
                Col = -1
            }
        };

        CurrentMatches.Add(newMatch);
        if (match.IsBotGame)
        {
            InitializeBotAI(userId, newMatch);
        }

        var currentUser = await Users.GetUserByIdAsync(userId);
        await Clients.Clients([newMatch.Player1Id, newMatch.Player2Id]).ReceiveMatchRestart(newMatch.Id, $"Player {currentUser?.Name ?? ""} restarted the match", newMatch);

        await Clients.Clients(newMatch.Viewers.Select(v => v.Id)).ReceiveMatchRestart(newMatch.Id, $"Player {currentUser?.Name ?? ""} restarted the match", newMatch, true);
        Logger.LogInformation("Match restarted: {MatchId} between {Player1Id} and {Player2Id}", newMatch.Id, newMatch.Player1Id, newMatch.Player2Id);
    }

    private static void InitializeBotAI(string userId, SimpleMatch newMatch)
    {
        var board2D = new int[newMatch.Board.Length, newMatch.Board[0].Length];
        for (int i = 0; i < newMatch.Board.Length; i++)
        {
            for (int j = 0; j < newMatch.Board[i].Length; j++)
            {
                board2D[i, j] = newMatch.Board[i][j] == null ? 0 : newMatch.Board[i][j] == true ? 1 : 2;
            }
        }
        BotService.InitializeBoard(userId, board2D, newMatch.Player1Id == userId ? 1 : 2, newMatch.IsBlockTwoSides);
    }

    public async Task TriggerBotMove(string matchId)
    {
        var userId = GetUserId();
        var match = CurrentMatches.FirstOrDefault(m => m.Id.ToString() == matchId && (m.Player1Id == userId || m.Player2Id == userId));
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

        if (!match.IsBotGame)
        {
            Logger.LogWarning("Match is not a bot game: {MatchId}", matchId);
            throw new HubException("Match is not a bot game.");
        }

        if ((match.Player1Id == userId && match.IsPlayer1Turn) || (match.Player2Id == userId && !match.IsPlayer1Turn))
        {
            Logger.LogWarning("It's not bot turn");
            throw new HubException("It's not bot turn.");
        }

        await GetBotBestMove(match);
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

        match.Board[int.Parse(row)][int.Parse(column)] = match.Player1Id == userId;
        match.PreviousMove = new SimpleMove
        {
            Row = int.Parse(row),
            Col = int.Parse(column),
        };
        Logger.LogInformation("User {0} made a move at {Row}, {Col}", userId, row, column);

        await Clients.Clients([match.Player1Id, match.Player2Id]).ReceiveMove(row, column, match.Player1Id == userId ? CellState.X.ToString() : CellState.O.ToString(), false);
        await Clients.Clients(match.Viewers.Select(v => v.Id)).ReceiveMove(row, column, !match.IsPlayer1Turn ? CellState.X.ToString() : CellState.O.ToString(), true);
    }

    private async Task GetBotBestMove(SimpleMatch match)
    {
        var boardJagged = match.Board.ToArray().Select(row => row.Select(cell => cell == null ? 0 : cell.Value ? 1 : 2).ToArray()).ToArray();
        var board = new int[boardJagged.Length, boardJagged[0].Length];
        for (int i = 0; i < boardJagged.Length; i++)
        {
            for (int j = 0; j < boardJagged[i].Length; j++)
            {
                board[i, j] = boardJagged[i][j];
            }
        }
        var userId = GetUserId();
        int botPlayer = userId == match.Player1Id ? 2 : 1;

        Logger.LogInformation("Bot is making a move for player {Player}", botPlayer);
        var botMove = BotService.GetMove(userId, new Point(match.PreviousMove.Row, match.PreviousMove.Col), botPlayer);
        if (botMove.r != -1 && botMove.c != -1)
        {
            match.Board[botMove.r][botMove.c] = botPlayer == 1;
            Logger.LogInformation("Bot move: {Row}, {Col}", botMove.r, botMove.c);
            match.PreviousMove = new SimpleMove
            {
                Row = botMove.r,
                Col = botMove.c,
            };
            await Clients.Clients([match.Player1Id, match.Player2Id]).ReceiveMove(botMove.r.ToString(), botMove.c.ToString(), match.IsPlayer1Turn ? CellState.X.ToString() : CellState.O.ToString(), false);
            await Clients.Clients(match.Viewers.Select(v => v.Id)).ReceiveMove(botMove.r.ToString(), botMove.c.ToString(), match.IsPlayer1Turn ? CellState.X.ToString() : CellState.O.ToString(), true);
            match.IsPlayer1Turn = !match.IsPlayer1Turn;
        }
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

    public async Task<SimplePlayingMatch> JoinMatch(string matchId)
    {
        var match = CurrentMatches.FirstOrDefault(m => m.Id.ToString() == matchId);
        if (match == null)
        {
            Logger.LogWarning("Match not found: {MatchId}", matchId);
            throw new HubException("Match not found.");
        }

        var userId = GetUserId();
        var currentUser = await Users.GetUserByIdAsync(userId);

        if (currentUser == null)
        {
            Logger.LogWarning("User not found: {UserId}", userId);
            throw new HubException("User not found.");
        }

        if (match.Result != MatchResult.Ongoing)
        {
            Logger.LogWarning("Match is not ongoing: {MatchId}", matchId);
            throw new HubException("Match has ended..");
        }

        if (IsUserInMatch(userId, matchId))
        {
            Logger.LogWarning("User {UserId} is currently in the match: {MatchId}", userId, matchId);
            throw new HubException("You are already in this match.");
        }

        match.Viewers.Add(currentUser);

        return new SimplePlayingMatch
        {
            Id = match.Id.ToString(),
            Player1 = new SimpleUser { Id = match.Player1Id, Name = match.Player1Name },
            Player2 = new SimpleUser { Id = match.Player2Id, Name = match.Player2Name },
            Board = match.Board,
            Row = match.Row,
            Column = match.Column,
            IsPlayer1Turn = match.IsPlayer1Turn,
            IsBlockTwoSides = match.IsBlockTwoSides,
            PreviousMove = match.PreviousMove,
            IsBotGame = match.IsBotGame,
        };
    }

    public Task<List<SimplePlayingMatch>> GetPlayingMatches()
    {
        return Task.FromResult(CurrentMatches.Where(m => m.Result == MatchResult.Ongoing).Select(m => new SimplePlayingMatch
        {
            Id = m.Id.ToString(),
            Player1 = new SimpleUser { Id = m.Player1Id, Name = m.Player1Name },
            Player2 = new SimpleUser { Id = m.Player2Id, Name = m.Player2Name },
            Board = m.Board,
            Row = m.Row,
            Column = m.Column,
            IsPlayer1Turn = m.IsPlayer1Turn,
            IsBlockTwoSides = m.IsBlockTwoSides,
            PreviousMove = m.PreviousMove,
            IsBotGame = m.IsBotGame,
        }).ToList());
    }

    public async Task ExitCurrentViewingMatch(string matchId)
    {
        var userId = GetUserId();
        var match = CurrentMatches.FirstOrDefault(m => m.Id.ToString() == matchId && m.Viewers.Any(v => v.Id == userId));
        if (match == null)
        {
            Logger.LogWarning("Match not found: {MatchId}", matchId);
            throw new HubException("Match not found.");
        }

        var currentUser = await Users.GetUserByIdAsync(userId);
        if (currentUser == null)
        {
            Logger.LogWarning("User not found: {UserId}", userId);
            throw new HubException("User not found.");
        }

        match.Viewers.Remove(currentUser);
    }

    public async Task<SimpleMatch> FindGameWithBot(int row, int column, bool isBlockTwoSides = false)
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
            return null!;
        }

        var botName = BotNameHelper.GetBotName();
        var botId = Guid.NewGuid().ToString();

        var match = new SimpleMatch
        {
            Id = Guid.NewGuid().ToString(),
            Row = row,
            Column = column,
            Player1Id = Context.ConnectionId,
            Player1Name = currentUser.Name,
            Player2Id = botId,
            Player2Name = botName,
            Board = [.. Enumerable.Range(0, row).Select(_ => new bool?[column])],
            Viewers = [],
            IsPlayer1Turn = true,
            Result = MatchResult.Ongoing,
            IsBlockTwoSides = isBlockTwoSides,
            WinnerId = string.Empty,
            IsBotGame = true,
        };

        CurrentMatches.Add(match);

        InitializeBotAI(Context.ConnectionId, match);

        await Clients.Clients(match.Player1Id).ReceiveMatchFound(botName, match);
        await Clients.Clients(match.Player2Id).ReceiveMatchFound(currentUser.Name, match);

        return match;
    }
}
