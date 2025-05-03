namespace TicTacToeGame.Services.Bot;

public class FirstEmptyCellBot
{
    public static Point GetBestMove(int[,] board, int player, bool blockTwoSides = false)
    {
        int opponent = player == 1 ? 2 : 1;

        // Check if the player can win in the next move
        Point? winningMove = CheckIsWinningNextMove(board, player);
        if (winningMove != null)
        {
            return winningMove; // Return the winning move
        }

        // Block the opponent if they can win in the next move
        Point? blockingMove = CheckIsWinningNextMove(board, opponent);
        if (blockingMove != null)
        {
            return blockingMove; // Block the opponent's winning move
        }

        // Otherwise, choose the first available empty cell
        Point? emptyCell = FindFirstEmptyCell(board);

        if (emptyCell != null)
        {
            return emptyCell; // Return the first empty cell
        }

        return new Point(-1, -1); // No valid move available
    }

    private static Point? FindFirstEmptyCell(int[,] board)
    {
        int rows = board.GetLength(0);
        int cols = board.GetLength(1);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (board[i, j] == 0)
                {
                    return new Point(i, j); // Return the first empty cell
                }
            }
        }

        return null; // No empty cell found
    }

    private static Point? CheckIsWinningNextMove(int[,] board, int player)
    {
        int rows = board.GetLength(0);
        int cols = board.GetLength(1);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (board[i, j] == 0)
                {
                    board[i, j] = player;
                    if (IsWinning(board, i, j, player))
                    {
                        board[i, j] = 0; // Revert the move
                        return new Point(i, j); // Return the winning move
                    }
                    board[i, j] = 0; // Revert the move
                }
            }
        }

        return null;
    }

    private static bool IsWinning(int[,] board, int row, int col, int player)
    {
        int rows = board.GetLength(0);
        int cols = board.GetLength(1);
        int neededToWin = Math.Min(rows, cols) >= 5 ? 5 : Math.Min(rows, cols);

        // Directions: horizontal, vertical, and two diagonals
        int[][] directions = new int[][] {
            [0, 1], // Horizontal
            [1, 0], // Vertical
            [1, 1], // Diagonal down-right
            [1, -1] // Diagonal down-left
        };

        foreach (var dir in directions)
        {
            int count = 1; // Count the current cell
            // Check in both horizontal and reverse horizontal directions
            // Check in both vertical and reverse vertical directions
            // Check in both diagonal directions and reverse diagonal directions
            for (int d = -1; d <= 1; d += 2)
            {
                // calculate the next cell in the direction
                int r = row + dir[0] * d,
                    c = col + dir[1] * d;

                // check if the next cell is within bounds and matches the player
                while (r >= 0 && r < rows && c >= 0 && c < cols && board[r, c] == player)
                {
                    count++;
                    if (count == neededToWin) // Check if we have 3 in a row
                    {
                        return true; // Winning condition met
                    }
                    // calculate the next cell in the same direction using the coordinates formula for calculating the next cell in a line
                    // d is a multiplier that can be -1 or 1 to check both directions
                    // d is a vector than present the direction of the line
                    r += dir[0] * d; // Formula: x = x0 + dx * d
                    c += dir[1] * d; // Formula: y = y0 + dy * d
                }
            }
        }

        return false; // No winning condition met
    }
}