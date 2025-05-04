using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

namespace TicTacToeGame.Services.Bot;

public class GomokuBot
{
    private const int DEFAULT_SEARCH_DEPTH = 3;
    private static readonly ConcurrentDictionary<ulong, (int score, int depth)> TranspositionTable = new();
    private static ZobristHash zobristHasher;

    // Directions for scanning lines: horiz, vert, diag \ and diag /
    private static readonly (int dr, int dc)[] Directions = {
        (0,1), (1,0), (1,1), (1,-1)
    };

    static GomokuBot()
    {
        // Initialize Zobrist hashing with random numbers
        zobristHasher = new ZobristHash(15, 15); // Assuming max 15x15 board
    }

    public static Point GetBestMove(int[,] board, int player, int depth = DEFAULT_SEARCH_DEPTH, bool blockTwoSides = false)
    {
        var stopwatch = Stopwatch.StartNew();
        var bestScore = int.MinValue;
        Point bestMove = new(-1, -1);
        var moves = GenerateCandidateMoves(board);

        // Try center first if board is empty
        if (moves.Count > 10 && board.Cast<int>().All(c => c == 0))
        {
            int centerR = board.GetLength(0) / 2;
            int centerC = board.GetLength(1) / 2;
            return new Point(centerR, centerC);
        }

        // Parallel move evaluation
        Parallel.ForEach(moves, (move, state) =>
        {
            board[move.r, move.c] = player;
            int score = Minimax(board, depth - 1, false, player, int.MinValue, int.MaxValue, blockTwoSides);
            board[move.r, move.c] = 0;

            lock (moves)
            {
                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;

                    // Early termination if we found a winning move
                    if (bestScore >= (int)GameScoreLine.Win * 0.9)
                    {
                        state.Break();
                    }
                }
            }
        });

        Debug.WriteLine($"Move calculation took {stopwatch.ElapsedMilliseconds}ms");
        return bestMove;
    }

    private static int Minimax(int[,] board, int depth, bool maximizing, int player, int alpha, int beta, bool blockTwoSides)
    {
        int conditionToWin = (int)(Math.Min(board.GetLength(0), board.GetLength(1)) switch
        {
            >= 5 => GameScoreLine.Win,
            4 => GameScoreLine.Close_Four,
            3 => GameScoreLine.Close_Three,
            2 => GameScoreLine.Close_Two,
            1 => GameScoreLine.Line_Score,
            _ => GameScoreLine.No_Score
        });

        int opponent = 3 - player;
        ulong boardHash = zobristHasher.Hash(board);

        // Check transposition table
        if (TranspositionTable.TryGetValue(boardHash, out var entry) && entry.depth >= depth)
        {
            return entry.score;
        }

        int eval = EvaluateBoard(board, maximizing ? player : opponent);
        int currentThreat = Math.Abs(eval);

        // Dynamic depth extension
        int extraDepth = currentThreat >= (int)GameScoreLine.Close_Four ? 2 :
                        currentThreat >= (int)GameScoreLine.Open_Three ? 1 : 0;

        // Terminal conditions
        if (depth + extraDepth <= 0 || currentThreat >= conditionToWin)
        {
            return eval;
        }

        var moves = GenerateCandidateMoves(board);
        if (moves.Count == 0) return 0;

        // Move ordering with priority queue
        var moveQueue = new PriorityQueue<Point, int>();
        foreach (var move in moves)
        {
            int priority = EvaluateMove(board, move.r, move.c, maximizing ? player : opponent);
            moveQueue.Enqueue(move, maximizing ? -priority : priority); // Higher priority first
        }

        int result;
        if (maximizing)
        {
            int maxEval = int.MinValue;
            while (moveQueue.TryDequeue(out var move, out _))
            {
                board[move.r, move.c] = player;
                int score = Minimax(board, depth - 1, false, player, alpha, beta, blockTwoSides);
                board[move.r, move.c] = 0;
                maxEval = Math.Max(maxEval, score);
                alpha = Math.Max(alpha, score);
                if (beta <= alpha) break;
            }
            result = maxEval;
        }
        else
        {
            int minEval = int.MaxValue;
            while (moveQueue.TryDequeue(out var move, out _))
            {
                board[move.r, move.c] = opponent;
                int score = Minimax(board, depth - 1, true, player, alpha, beta, blockTwoSides);
                board[move.r, move.c] = 0;
                minEval = Math.Min(minEval, score);
                beta = Math.Min(beta, score);
                if (beta <= alpha) break;
            }
            result = minEval;
        }

        // Store in transposition table
        TranspositionTable[boardHash] = (result, depth);
        return result;
    }

    private static int EvaluateMove(int[,] board, int row, int col, int player)
    {
        board[row, col] = player;
        int score = EvaluateBoard(board, player);
        board[row, col] = 0;
        return score;
    }

    // Add this class for Zobrist hashing
    private class ZobristHash
    {
        private readonly ulong[,,] table;
        private readonly ulong[] playerToMove;

        public ZobristHash(int maxRows, int maxCols)
        {
            var random = new Random();
            table = new ulong[maxRows, maxCols, 3]; // 0=empty, 1=player1, 2=player2
            playerToMove = new ulong[3];

            for (int r = 0; r < maxRows; r++)
                for (int c = 0; c < maxCols; c++)
                    for (int p = 0; p < 3; p++)
                        table[r, c, p] = (ulong)random.NextInt64();

            for (int p = 0; p < 3; p++)
                playerToMove[p] = (ulong)random.NextInt64();
        }

        public ulong Hash(int[,] board)
        {
            ulong hash = 0;
            for (int r = 0; r < board.GetLength(0); r++)
                for (int c = 0; c < board.GetLength(1); c++)
                    hash ^= table[r, c, board[r, c]];
            return hash;
        }
    }

    // Rest of your existing methods (EvaluateBoard, ScoreLine, GenerateCandidateMoves, etc.)
    // can remain largely the same, but consider adding incremental evaluation

    /// <summary>
    /// Heuristic evaluation: scans the board, sums up pattern scores.
    /// Big positive → good for 'player'; big negative → good for opponent.
    /// </summary>
    /// <param name="board">The current state of the board.</param>
    /// <param name="player">The player number (1 or 2).</param>
    private static int EvaluateBoard(int[,] board, int player)
    {
        int rows = board.GetLength(0),
            cols = board.GetLength(1);
        int score = 0;

        // Scan every cell in every direction
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (board[r, c] == 0)
                {
                    continue; // Skip empty cells
                }
                int who = board[r, c];
                foreach (var (dr, dc) in Directions)
                {
                    score += ScoreLine(board, r, c, dr, dc, who == player, player); // Score the line
                }
            }
        }

        return score; // Return the total score
    }

    /// <summary>
    /// Score a a line starting at (r,c) in direction (dr, dc) for 'isPlayer'.
    /// </summary>
    /// <param name="board">The current state of the board.</param>
    /// <param name="r">Row index of the move.</param>
    /// <param name="c">Column index of the move.</param>
    /// <param name="dr">Delta row of the move.</param>
    /// <param name="dc">Delta column of the move.</param>
    /// <param name="isPlayer">Player or opponent</param>
    /// <returns>The Scoreline</returns>
    private static int ScoreLine(int[,] board, int r, int c, int dr, int dc, bool isPlayer, int player)
    {
        int opponent = 3 - player; // Switch player (1 -> 2, 2 -> 1) (because 1 + 2 = 3)
        int me = isPlayer ? player : opponent; // Determine the player

        // Count consecutive stones of 'me'
        int count = 0,
            rr = r,
            cc = c;
        while (IsValidMoveOfPlayer(board, rr, cc, me))
        {
            count++;
            rr += dr;
            cc += dc;
        }

        // Stop if this segment is too short to matter
        if (count == 0) return 0;

        // Check ends
        bool openStart = false,
            openEnd = false;

        // Check cell before the start of the segment
        int sr = r - dr,
            sc = c - dc;

        if (IsValidEmptyMove(board, sr, sc))
        {
            openStart = true; // Open at start
        }

        // Check cell after the segment
        if (IsValidEmptyMove(board, rr, cc))
        {
            openEnd = true; // Open at end
        }

        // Assign heuristic values
        // Special case: handle lines with 4 or more stones
        if (count >= 4)
        {
            return count >= 5
            ? (isPlayer ? (int)GameScoreLine.Win : -(int)GameScoreLine.Win) // Immediate win for 5+ stones
            : (openStart && openEnd
                ? (isPlayer ? (int)GameScoreLine.Open_Four : -(int)GameScoreLine.Open_Four) // Open four
                : (isPlayer ? (int)GameScoreLine.Close_Four : -(int)GameScoreLine.Close_Four)); // Closed four
        }

        // Handle shorter lines with heuristic scoring
        int lineScore = count switch
        {
            3 when openStart && openEnd => (int)GameScoreLine.Open_Three,
            3 when openStart || openEnd => (int)GameScoreLine.Close_Three,
            2 when openStart && openEnd => (int)GameScoreLine.Open_Two,
            2 when openStart || openEnd => (int)GameScoreLine.Close_Two,
            1 when openStart && openEnd => (int)GameScoreLine.Line_Score,
            _ => (int)GameScoreLine.No_Score
        };

        return isPlayer ? lineScore : -lineScore;
    }

    /// <summary>
    /// Generates a list of of empty cells that are within 2 cells of an existing stone.
    /// This greatly prune the branching factor on large boards.
    /// </summary>
    /// <param name="board">The current state of the board.</param>
    /// <returns>A list of candidate moves.</returns>
    private static List<Point> GenerateCandidateMoves(int[,] board)
    {
        int rows = board.GetLength(0);
        int cols = board.GetLength(1);
        var candidates = new HashSet<Point>();
        // HashSet to avoid duplicates
        int range = 2;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (board[r, c] == 0)
                {
                    continue; // Skip empty cells
                }

                for (int dr = -range; dr <= range; dr++)
                {
                    for (int dc = -range; dc <= range; dc++)
                    {
                        int rr = r + dr,
                            cc = c + dc;

                        if (IsValidEmptyMove(board, rr, cc))
                        {
                            if (candidates.FirstOrDefault(p => p.r == rr && p.c == cc) is not null)
                            {
                                continue; // Skip if already in candidates
                            }
                            candidates.Add(new Point(rr, cc)); // Add valid candidate move
                        }
                    }
                }
            }
        }

        // If board is empty, just play in the center
        if (candidates.Count == 0 && board[rows / 2, cols / 2] == 0)
        {
            candidates.Add(new Point(rows / 2, cols / 2)); // Add center cell as candidate
        }

        return [.. candidates]; // Convert HashSet to List and return
    }

    /// <summary>
    /// Checks if the move is valid: within bounds and the cell is empty.
    /// </summary>TI
    /// <param name="board">The current state of the board.</param>
    /// <param name="r">Row index of the move.</param>
    /// <param name="c">Column index of the move.</param>
    /// <returns>True if the move is valid, false otherwise.</returns>
    /// <remarks>Assumes the board is rectangular.</remarks>
    private static bool IsValidEmptyMove(int[,] board, int r, int c)
    {
        // Validate the move: check if within bounds and if the cell is empty
        return r >= 0 && r < board.GetLength(0) && c >= 0 && c < board.GetLength(1) && board[r, c] == 0;
    }

    /// <summary>
    /// Checks if the move is valid and belongs to a specific player.
    /// </summary>
    /// <param name="board">The current state of the board.</param>
    /// <param name="r">Row index of the move.</param>
    /// <param name="c">Column index of the move.</param>
    /// <param name="player">The player number (1 or 2).</param>
    /// <returns>True if the move is within bounds and the cell belongs to the player.</returns>
    private static bool IsValidMoveOfPlayer(int[,] board, int r, int c, int player)
    {
        int rows = board.GetLength(0);
        int cols = board.GetLength(1);

        return r >= 0 && r < rows &&
               c >= 0 && c < cols &&
               board[r, c] == player;
    }
}

public enum GameScoreLine
{
    Win = 10_000_000,
    Open_Four = 5_000_000,    // Slightly reduced
    Close_Four = 3_000_000,   // Increased (can force wins)
    Open_Three = 300_000,     // More aggressive
    Close_Three = 50_000,
    Open_Two = 2_000,
    Close_Two = 200,
    Line_Score = 1,
    No_Score = 0
}