using System.Collections.Concurrent;

namespace TicTacToeGame.Services.Bot;

public class Zobrist
{
    public static readonly Dictionary<int, ulong[,,]> CachedTables = [];
    private static readonly Random Rand = new();
    private ulong[,,] _table;

    public ulong[,,] Table
    {
        get
        {
            if (_table == null)
                throw new InvalidOperationException("Zobrist table not initialized");
            return _table;
        }
        private set => _table = value;
    }

    public void Init(int rows, int cols)
    {
        // var cachedTable = CachedTables.GetValueOrDefault(rows);
        // if (cachedTable != null)
        // {
        //     Table = (ulong[,,])cachedTable.Clone();
        //     return;
        // }

        Table = new ulong[rows, cols, 3];
        // CachedTables[rows] = (ulong[,,])Table.Clone();

        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                for (int p = 0; p < 3; p++)
                    Table[r, c, p] = ((ulong)(uint)Rand.Next() << 32) | (uint)Rand.Next();
    }

    public ulong ComputeHash(int[,] board)
    {
        ulong hash = 0;
        int rows = board.GetLength(0), cols = board.GetLength(1);
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
            {
                int v = board[r, c];
                if (v != 0)
                    hash ^= Table[r, c, v];
            }
        return hash;
    }

    public ulong UpdateHash(ulong hash, int row, int col, int player)
    {
        return hash ^ Table[row, col, player];
    }
}

public class TranspositionTable
{
    private readonly ConcurrentDictionary<ulong, int> table = new();

    public void Store(ulong hash, int value) => table[hash] = value;
    public bool TryGet(ulong hash, out int value) => table.TryGetValue(hash, out value);
    public void Clear() => table.Clear();
}

public class GomokuAI
{
    private int[,] board;
    private int player;
    private System.Drawing.Point bestMove;
    private readonly TranspositionTable transTable;
    private readonly Zobrist Zobrist = new();
    private ulong currentHash;
    private readonly int rows, cols;
    private readonly bool BlockTwoSides;
    private readonly ConcurrentDictionary<(int count, bool openStart, bool openEnd), int> _patternCache = new();

    private static readonly (int dr, int dc)[] Directions = {
        (0, 1), (1, 0), (1, 1), (1, -1)
    };

    public GomokuAI(int[,] board, int player, bool BlockTwoSides = false)
    {
        this.board = (int[,])board.Clone();  // Create a copy of the board
        this.player = player;
        this.rows = board.GetLength(0);
        this.cols = board.GetLength(1);
        this.BlockTwoSides = BlockTwoSides;

        // Initialize Zobrist table only if needed
        Zobrist.Init(rows, cols);

        this.transTable = new TranspositionTable();
        this.currentHash = Zobrist.ComputeHash(this.board);
    }

    public void UpdateBoard(Point move, int playerWhoMoved)
    {
        int row = move.r,
            col = move.c;
        // Update the board
        board[row, col] = playerWhoMoved;

        // Update the hash incrementally
        currentHash = Zobrist.UpdateHash(currentHash, row, col, playerWhoMoved);

        // Update the current player (assuming it alternates)
        player = 3 - player;
    }

    public Point GetMove()
    {
        var immediate = GetImmediateMove(board, player);
        if (immediate != null)
            return new Point(immediate.Value.Y, immediate.Value.X);

        bestMove = new System.Drawing.Point(-1, -1);
        int maxScore = int.MinValue;

        var moves = GenerateCandidateMoves(board)
            .Where(p => board[p.Y, p.X] == 0)
            .OrderByDescending(p => MoveHeuristic(board, p.Y, p.X, player))
            .ToList();

        object lockObj = new object();

        Parallel.ForEach(moves, move =>
        {
            int[,] clone = (int[,])board.Clone();
            ulong hash = currentHash;
            clone[move.Y, move.X] = player;
            ulong newHash = Zobrist.UpdateHash(hash, move.Y, move.X, player);
            int score = Minimax(clone, 3, false, 3 - player, int.MinValue, int.MaxValue, newHash);

            lock (lockObj)
            {
                if (score > maxScore)
                {
                    maxScore = score;
                    bestMove = move;
                }
            }
        });

        return new Point(bestMove.Y, bestMove.X);
    }

    private System.Drawing.Point? GetImmediateMove(int[,] b, int currentPlayer)
    {
        var moves = GenerateCandidateMoves(b);
        foreach (var move in moves)
        {
            if (b[move.Y, move.X] != 0) continue;
            b[move.Y, move.X] = currentPlayer;
            if (CheckWin(b, move.Y, move.X, currentPlayer))
            {
                b[move.Y, move.X] = 0;
                return move;
            }
            b[move.Y, move.X] = 0;
        }

        int opponent = 3 - currentPlayer;
        foreach (var move in moves)
        {
            if (b[move.Y, move.X] != 0) continue;
            b[move.Y, move.X] = opponent;
            if (CheckWin(b, move.Y, move.X, opponent))
            {
                b[move.Y, move.X] = 0;
                return move;
            }
            b[move.Y, move.X] = 0;
        }

        return null;
    }

    private bool CheckWin(int[,] board, int row, int col, int player)
    {
        foreach (var (dr, dc) in Directions)
        {
            int count = 1; // Count the placed stone

            // Count consecutive stones in positive direction
            int positiveEndR = row, positiveEndC = col;
            for (int i = 1; i <= 4; i++)
            {
                int r = row + dr * i;
                int c = col + dc * i;

                if (r < 0 || r >= rows || c < 0 || c >= cols || board[r, c] != player)
                    break;

                count++;
                positiveEndR = r;
                positiveEndC = c;
            }

            // Count consecutive stones in negative direction
            int negativeEndR = row, negativeEndC = col;
            for (int i = 1; i <= 4; i++)
            {
                int r = row - dr * i;
                int c = col - dc * i;

                if (r < 0 || r >= rows || c < 0 || c >= cols || board[r, c] != player)
                    break;

                count++;
                negativeEndR = r;
                negativeEndC = c;
            }

            // Must have exactly 5 in a row AND at least one open end
            if (count == 5)
            {
                if (!BlockTwoSides)
                {
                    return true;
                }

                // Check if positive end is open
                int nextR = positiveEndR + dr;
                int nextC = positiveEndC + dc;
                bool positiveOpen = (nextR >= 0 && nextR < rows && nextC >= 0 && nextC < cols && board[nextR, nextC] == 0);

                // Check if negative end is open
                int prevR = negativeEndR - dr;
                int prevC = negativeEndC - dc;
                bool negativeOpen = (prevR >= 0 && prevR < rows && prevC >= 0 && prevC < cols && board[prevR, prevC] == 0);

                // Win only if at least one end is open
                if (positiveOpen || negativeOpen)
                    return true;
            }
        }
        return false;
    }

    private int Minimax(int[,] b, int depth, bool maximizing, int currentPlayer, int alpha, int beta, ulong hash)
    {
        if (transTable.TryGet(hash, out int cached))
            return cached;

        var moves = GenerateCandidateMoves(b)
            .Where(p => b[p.Y, p.X] == 0)
            .OrderByDescending(p => MoveHeuristic(b, p.Y, p.X, currentPlayer))
            .ToList();

        if (depth == 0 || moves.Count == 0)
            return EvaluateBoard(b, player);

        int bestScore = maximizing ? int.MinValue : int.MaxValue;
        int opponent = 3 - currentPlayer;

        foreach (var move in moves)
        {
            b[move.Y, move.X] = currentPlayer;
            ulong newHash = Zobrist.UpdateHash(hash, move.Y, move.X, currentPlayer);
            int score = Minimax(b, depth - 1, !maximizing, player, alpha, beta, newHash);
            b[move.Y, move.X] = 0;

            if (maximizing)
            {
                bestScore = Math.Max(bestScore, score);
                alpha = Math.Max(alpha, score);
            }
            else
            {
                bestScore = Math.Min(bestScore, score);
                beta = Math.Min(beta, score);
            }

            if (beta <= alpha) break;
        }

        if (depth >= 2) transTable.Store(hash, bestScore);
        return bestScore;
    }

    private int MoveHeuristic(int[,] b, int r, int c, int player)
    {
        int score = 0;
        foreach (var (dr, dc) in Directions)
        {
            for (int i = -1; i <= 1; i += 2)
            {
                int rr = r + dr * i, cc = c + dc * i;
                if (rr >= 0 && rr < rows && cc >= 0 && cc < cols && b[rr, cc] == player)
                    score++;
            }
        }
        return score;
    }

    private int EvaluateBoard(int[,] b, int me)
    {
        int score = 0;
        int opp = 3 - me;
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
            {
                if (b[r, c] == 0) continue;
                bool isPlayer = b[r, c] == me;
                foreach (var (dr, dc) in Directions)
                    score += ScoreLine(b, r, c, dr, dc, isPlayer);
            }
        return score;
    }

    private int ScoreLine(int[,] b, int r, int c, int dr, int dc, bool isPlayer)
    {
        int me = isPlayer ? player : 3 - player;
        int count = 0, rr = r, cc = c;
        while (rr >= 0 && rr < rows && cc >= 0 && cc < cols && b[rr, cc] == me)
        {
            count++;
            rr += dr; cc += dc;
        }

        bool openStart = false, openEnd = false;

        int sr = r - dr, sc = c - dc;
        if (sr >= 0 && sr < rows && sc >= 0 && sc < cols && b[sr, sc] == 0) openStart = true;
        if (rr >= 0 && rr < rows && cc >= 0 && cc < cols && b[rr, cc] == 0) openEnd = true;

        // Create pattern key
        var patternKey = (count, openStart, openEnd);

        // Try to get from cache
        if (_patternCache.TryGetValue(patternKey, out int cachedScore))
        {
            return isPlayer ? cachedScore : -cachedScore;
        }

        int lineScore;
        if (count >= 5) lineScore = 1_100_000;
        else if (count == 4 && openStart && openEnd) lineScore = 1_000_000;
        else if (count == 4 && (openStart || openEnd)) lineScore = 700_000;
        else if (count == 3 && openStart && openEnd) lineScore = 500_000;
        else if (count == 3 && (openStart || openEnd)) lineScore = 10_000;
        else if (count == 2 && openStart && openEnd) lineScore = 100;
        else if (count == 2 && (openStart || openEnd)) lineScore = 10;
        else lineScore = 1;

        _patternCache[patternKey] = lineScore;

        return isPlayer ? lineScore : -lineScore;
    }

    private HashSet<System.Drawing.Point> GenerateCandidateMoves(int[,] b)
    {
        var moves = new HashSet<System.Drawing.Point>();
        int margin = 2;
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                if (b[r, c] != 0)
                    for (int dr = -margin; dr <= margin; dr++)
                        for (int dc = -margin; dc <= margin; dc++)
                        {
                            int rr = r + dr, cc = c + dc;
                            if (rr >= 0 && rr < rows && cc >= 0 && cc < cols && b[rr, cc] == 0)
                            {
                                if (moves.Any(m => m.X == cc && m.Y == rr)) continue;
                                moves.Add(new System.Drawing.Point(cc, rr));
                            }
                        }

        if (moves.Count == 0 && b[rows / 2, cols / 2] == 0)
            moves.Add(new System.Drawing.Point(cols / 2, rows / 2));

        return moves;
    }
}