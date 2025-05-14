namespace TicTacToeGame.Services.Bot;
public class GomokuPatternDetector
{
    private int boardSize;
    private int[,] board;
    private bool xCheck;
    private bool oCheck;
    private int xChecksExpireIn;
    private int oChecksExpireIn;
    private List<Pattern> patterns;

    public GomokuPatternDetector(int boardSize = 15)
    {
        this.boardSize = boardSize;
        board = new int[boardSize, boardSize];
        xCheck = false;
        oCheck = false;
        xChecksExpireIn = 0;
        oChecksExpireIn = 0;
        patterns = [];
    }

    public class Pattern
    {
        public int Player { get; set; } = default!;
        public List<Tuple<int, int>> Positions { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public int Strength { get; set; }
    }

    private void UpdateChecks()
    {
        // Decrement white check counter if active
        if (xChecksExpireIn > 0)
        {
            xChecksExpireIn--;
            if (xChecksExpireIn == 0)
            {
                xCheck = false;
            }
        }

        // Decrement black check counter if active
        if (oChecksExpireIn > 0)
        {
            oChecksExpireIn--;
            if (oChecksExpireIn == 0)
            {
                oCheck = false;
            }
        }
    }

    public void ProcessMove(int x, int y, int player)
    {
        // Validate move coordinates
        if (x < 0 || x >= boardSize || y < 0 || y >= boardSize)
        {
            throw new ArgumentException("Invalid move coordinates");
        }

        // Update the board
        board[x, y] = player;

        // Update check status based on the move
        if (player == 1)
        {
            xCheck = true;
            xChecksExpireIn = 5;
        }
        else if (player == 2)
        {
            oCheck = true;
            oChecksExpireIn = 5;
        }

        // Update check expiration counters
        UpdateChecks();

        // Check for potential patterns when there's an active check
        if (HasMinimumPositionsRemaining(4))
        {
            if ((xCheck && !oCheck) || (!xCheck && oCheck))
            {
                if (CountChecks() >= 2)
                {
                    Pattern pattern = CreatePattern();
                    patterns.Add(pattern);
                }
            }
        }
    }

    private bool HasMinimumPositionsRemaining(int minPositions)
    {
        int emptyCount = 0;
        for (int i = 0; i < boardSize; i++)
        {
            for (int j = 0; j < boardSize; j++)
            {
                if (board[i, j] == 0)
                {
                    emptyCount++;
                }
            }
        }
        return emptyCount >= minPositions;
    }

    private int CountChecks()
    {
        int activeColor = xCheck ? 1 : 2;
        int maxCount = 0;

        // Define all possible directions to check
        Tuple<int, int>[] directions = {
            Tuple.Create(1, 0),   // Horizontal
            Tuple.Create(0, 1),   // Vertical
            Tuple.Create(1, 1),    // Diagonal down-right
            Tuple.Create(1, -1)    // Diagonal down-left
        };

        foreach (var direction in directions)
        {
            int dx = direction.Item1;
            int dy = direction.Item2;

            for (int x = 0; x < boardSize; x++)
            {
                for (int y = 0; y < boardSize; y++)
                {
                    if (board[x, y] == activeColor)
                    {
                        int currentCount = 1;
                        int nx = x + dx;
                        int ny = y + dy;

                        while (nx >= 0 && nx < boardSize && ny >= 0 && ny < boardSize &&
                               board[nx, ny] == activeColor)
                        {
                            currentCount++;
                            nx += dx;
                            ny += dy;
                        }

                        maxCount = Math.Max(maxCount, currentCount);
                    }
                }
            }
        }

        return maxCount;
    }

    private Pattern CreatePattern()
    {
        int activePlayer = xCheck ? 1 : 2;
        var patternPositions = new List<Tuple<int, int>>();

        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                if (board[x, y] == activePlayer)
                {
                    patternPositions.Add(Tuple.Create(x, y));
                }
            }
        }

        return new Pattern
        {
            Player = activePlayer,
            Positions = patternPositions,
            CreatedAt = DateTime.Now,
            Strength = CountChecks()
        };
    }

    public List<Pattern> GetDetectedPatterns()
    {
        return [.. patterns];
    }
}