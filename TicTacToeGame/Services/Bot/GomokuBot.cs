namespace TicTacToeGame.Services.Bot;

public class GomokuBot
{
    private const int DEFAULT_SEARCH_DEPTH = 4;

    // Directions for scanning lines: horiz, vert, diag \ and diag /
    private static readonly (int dr, int dc)[] Directions = {
        (0,1), (1,0), (1,1), (1,-1)
    };

    /// <summary>
    /// Entry point: returns the best move (r = row, c = column) for the 'player' 1 or 2.
    /// </summary>
    /// <param name="board">The current state of the board.</param>
    /// <param name="player">The player number (1 or 2).</param>
    /// <param name="depth">The search depth for the AI.</param>
    /// <param name="blockTwoSides">Whether to block two sides.</param>
    /// <returns>The best move as a Point object.</returns>
    public static Point GetBestMove(int[,] board, int player, int depth = DEFAULT_SEARCH_DEPTH, bool blockTwoSides = false)
    {
        var bestScore = int.MinValue;
        Point bestMove = new(-1, -1);
        var moves = GenerateCandidateMoves(board);

        foreach (var move in moves)
        {
            board[move.r, move.c] = player; // Simulate the move
            int score = Minimax(board, depth - 1, false, player, int.MinValue, int.MaxValue, blockTwoSides);
            board[move.r, move.c] = 0; // Undo the move

            if (score > bestScore)
            {
                bestScore = score; // Update the best score
                bestMove = move; // Update the best move
            }
        }

        return bestMove; // Return the best move found
    }

    /// <summary> 
    /// Minimax with α–β pruning. 
    /// </summary>
    /// <param name="board">The current state of the board.</param>
    /// <param name="depth">The current depth of the search.</param>
    /// <param name="maximizing">True if maximizing player, false if minimizing.</param>
    /// <param name="player">The player number (1 or 2).</param>
    /// <param name="alpha">The minimum value for α–β pruning.</param>
    /// <param name="beta">The maximum value for α–β pruning.</param>
    /// <param name="blockTwoSides">Whether to block two sides.</param>
    /// <returns>The score of the board state.</returns>
    /// <remarks>Assumes the board is rectangular.</remarks>
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
        int opponent = 3 - player; // Switch player (1 -> 2, 2 -> 1) (because 1 + 2 = 3)
        // Terminal evaluation of deptph limit
        int eval = EvaluateBoard(board, player);
        // Check if the winning condition is met ( > Immediate Win = 10_000_000)
        if (depth == 0 || Math.Abs(eval) >= conditionToWin)
        {
            return eval; // Return the evaluation score
        }

        var moves = GenerateCandidateMoves(board); // Generate candidate moves
        if (moves.Count == 0)
        {
            return 0; // draw
        }

        if (maximizing)
        {
            int maxEval = int.MinValue;
            foreach (var move in moves)
            {
                board[move.r, move.c] = player; // Simulate the move
                int score = Minimax(board, depth - 1, false, player, alpha, beta, blockTwoSides);
                board[move.r, move.c] = 0;
                maxEval = Math.Max(maxEval, score); // Update the maximum evaluation
                alpha = Math.Max(alpha, score); // Update alpha
                if (beta <= alpha)
                { // alpha >= beta which means we can prune the search tree
                    break; // Beta cut-off
                }
            }

            return maxEval; // Return the maximum evaluation
        }
        else
        {
            int minEval = int.MaxValue;
            foreach (var move in moves)
            {
                board[move.r, move.c] = opponent; // Simulate the move
                int score = Minimax(board, depth - 1, true, player, alpha, beta, blockTwoSides);
                board[move.r, move.c] = 0; // Undo the move
                minEval = Math.Min(minEval, score); // Update the minimum evaluation
                beta = Math.Min(beta, score); // Update beta
                if (beta <= alpha)
                { // alpha >= beta which means we can prune the search tree
                    break; // Alpha cut-off
                }
            }

            return minEval; // Return the minimum evaluation
        }
    }

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