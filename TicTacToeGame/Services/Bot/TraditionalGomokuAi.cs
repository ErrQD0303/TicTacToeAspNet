

namespace TicTacToeGame.Services.Bot;

public class TraditionalGomokuAI
{
    private int[,] Board { get; set; }
    private int Player { get; set; }
    private int Opponent { get; set; }
    private bool BlockTwoSides { get; set; }
    private const int WinScore = 100000;
    private const int LoseScore = -100000;
    private const int MaxDepth = 5;

    public TraditionalGomokuAI(int[,] board, int player, bool blockTwoSides = false)
    {
        Board = board;
        Player = player;
        Opponent = 3 - player; // Assuming players are 1 and 2
        BlockTwoSides = blockTwoSides;
    }

    public Point GetMove()
    {
        int bestScore = int.MinValue;
        Point bestMove = new(-1, -1);

        foreach (var move in GetAvailableMoves(Board))
        {
            Board[move.r, move.c] = Player; // Make the move
            int score = Minimax(Board, MaxDepth, false, int.MinValue, int.MaxValue, Player);
            Board[move.r, move.c] = 0; // Undo the move

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }

        return bestMove;
    }

    private int Minimax(int[,] board, int depth, bool isMaximizing, int alpha, int beta, int player)
    {
        int opponent = 3 - player; // Assuming players are 1 and 2
        if (IsTerminal(board) || depth == 0)
        {
            return Evaluate(board);
        }

        if (isMaximizing)
        {
            int maxEval = int.MinValue;
            foreach (var move in GetAvailableMoves(board))
            {
                board[move.r, move.c] = player; // Make the move
                int eval = Minimax(board, depth - 1, false, alpha, beta, player);
                board[move.r, move.c] = 0; // Undo the move

                alpha = Math.Max(maxEval, eval);

                if (beta <= alpha)
                {
                    break;
                }

            }

            return alpha;
        }

        int minEval = int.MaxValue;
        foreach (var move in GetAvailableMoves(board))
        {
            board[move.r, move.c] = Opponent; // Make the move
            int eval = Minimax(board, depth - 1, true, alpha, beta, opponent);
            board[move.r, move.c] = 0; // Undo the move

            beta = Math.Min(minEval, eval);

            if (beta <= alpha)
            {
                break; // Beta cut-off
            }

        }
        return beta;
    }

    private int Evaluate(int[,] board)
    {
        if (HasWon(board, Player, BlockTwoSides))
        {
            return WinScore;
        }
        if (HasWon(board, Opponent, BlockTwoSides))
        {
            return LoseScore;
        }
        return 0; // Neutral score if no one has won yet
    }

    private static bool IsTerminal(int[,] board)
    {
        return HasWon(board, 1) || HasWon(board, 2) || GetAvailableMoves(board).Count == 0;
    }

    private static List<Point> GetAvailableMoves(int[,] board)
    {
        HashSet<Point> moves = [];
        int rows = board.GetLength(0),
            cols = board.GetLength(1);

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (board[r, c] == 0)
                {
                    continue; // Skip occupied cells
                }

                for (int dr = -2; dr <= 2; dr++)
                {
                    for (int dc = -2; dc <= 2; dc++)
                    {
                        int nr = r + dr, nc = c + dc;
                        if (nr >= 0 && nr < rows && nc >= 0 && nc < cols && board[nr, nc] == 0)
                        {
                            if (!moves.Any(m => m.r == nr && m.c == nc))
                            {
                                moves.Add(new Point(nr, nc)); // Add available move
                            }
                        }
                    }
                }
            }
        }

        if (moves.Count == 0 && board[rows / 2, cols / 2] == 0)
        {
            moves.Add(new Point(rows / 2, cols / 2)); // Center cell if available
        }

        return [.. moves]; // Convert to List for easier manipulation
    }

    private static bool HasWon(int[,] board, int player, bool blockTwoSides = false)
    {
        int size = board.GetLength(0);

        for (int r = 0; r < size; r++)
        {
            for (int c = 0; c < size; c++)
            {
                if (CheckDirection(board, r, c, 0, 1, player, blockTwoSides)) return true; // horizontal
                if (CheckDirection(board, r, c, 1, 0, player, blockTwoSides)) return true; // vertical
                if (CheckDirection(board, r, c, 1, 1, player, blockTwoSides)) return true; // diagonal
                if (CheckDirection(board, r, c, 1, -1, player, blockTwoSides)) return true; // anti-diagonal
            }
        }
        return false;
    }

    private static bool CheckDirection(int[,] board, int row, int col, int dRow, int dCol, int player, bool blockTwoSides)
    {
        if (board[row, col] != player) return false;

        int count = 1;
        for (int i = 1; i < 5; i++)
        {
            int r = row + dRow * i;
            int c = col + dCol * i;
            if (r >= 0 && c >= 0 && r < board.GetLength(0) && c < board.GetLength(1) && board[r, c] == player)
            {
                count++;
            }
            else break;
        }

        if (count == 5) return true;

        if (blockTwoSides && count == 4)
        {
            return CheckBlockTwoSides(board, row, col, dRow, dCol, player);
        }

        return false;
    }

    private static bool CheckBlockTwoSides(int[,] board, int row, int col, int dRow, int dCol, int player)
    {
        int count = 0;
        for (int i = -1; i <= 1; i += 2)
        {
            int r = row + dRow * i;
            int c = col + dCol * i;
            if (r >= 0 && c >= 0 && r < board.GetLength(0) && c < board.GetLength(1) && board[r, c] == player)
            {
                count++;
            }
        }
        return count == 2;
    }
}