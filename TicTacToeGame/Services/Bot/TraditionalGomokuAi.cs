using System.Collections.Concurrent;

namespace TicTacToeGame.Services.Bot;

public class TraditionalGomokuAI
{
    private int[,] _board = default!;
    private int[,] Board
    {
        get => _board;
        set
        {
            _board = value;
            Rows = _board.GetLength(0);
            Cols = _board.GetLength(1);
            InitializeZobristTable();
        }
    }

    private int Rows { get; set; }
    private int Cols { get; set; }
    private int Player { get; set; }
    private int Opponent { get; set; }
    private bool BothSidesBlock { get; set; }
    private const int MaxDepth = 5;

    private static readonly ListStonePattern ListStoneForPlayer1 = ListStonePattern.Create(true);
    private static readonly ListStonePattern ListStoneForPlayer2 = ListStonePattern.Create(false);

    // Zobrist hashing components
    private static ConcurrentDictionary<string, ulong[,]> CachedZobristTable { get; set; } = default!;
    private ulong[,] zobristTable = default!;
    private ulong zobristHash;
    private readonly Dictionary<ulong, TranspositionEntry> transpositionTable = new();

    private static readonly (int, int)[] Directions =
    {
        (0, 1),   // Horizontal
        (1, 0),   // Vertical
        (1, 1),   // Diagonal
        (1, -1)   // Anti-diagonal
    };

    static TraditionalGomokuAI()
    {
        CachedZobristTable = new ConcurrentDictionary<string, ulong[,]>();
    }

    public TraditionalGomokuAI(int[,] board, int player, bool bothSidesBlock = false)
    {
        Board = board;
        Player = player;
        Opponent = 3 - player; // Assuming players are 1 and 2
        BothSidesBlock = bothSidesBlock;
        InitializeZobristHash();
    }

    private void InitializeZobristTable()
    {
        CachedZobristTable ??= [];

        string key = $"{Rows},{Cols}";
        if (CachedZobristTable != null && CachedZobristTable.TryGetValue(key, out var cachedTable))
        {
            zobristTable = cachedTable;
            return;
        }

        var random = new Random();
        zobristTable = new ulong[Rows * Cols, 3]; // 3 possible values: 0 (empty), 1 (player1), 2 (player2)

        for (int i = 0; i < Rows * Cols; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                byte[] buf = new byte[8];
                random.NextBytes(buf);
                zobristTable[i, j] = BitConverter.ToUInt64(buf, 0);
            }
        }
        CachedZobristTable![$"{Rows},{Cols}"] = zobristTable;
    }

    private void InitializeZobristHash()
    {
        zobristHash = 0;
        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Cols; c++)
            {
                int piece = Board[r, c];
                zobristHash ^= zobristTable[r * Cols + c, piece];
            }
        }
    }

    private void UpdateZobristHash(int r, int c, int oldPiece, int newPiece)
    {
        zobristHash ^= zobristTable[r * Cols + c, oldPiece];
        zobristHash ^= zobristTable[r * Cols + c, newPiece];
    }

    public Point GetMove()
    {
        transpositionTable.Clear();
        double bestScore = double.MinValue;
        Point bestMove = new(-1, -1);

        var moves = GetCandidateMoves(Board, 2)
            .Select(m => new { Move = m, Score = Heuristic(Board, m, false) })
            .OrderByDescending(x => x.Score)
            // .Take(10)
            .Select(x => x.Move)
            .ToList();

        if (moves.Count == 0) return bestMove;
        if (moves.Count == 1) return moves[0];

        // Check for winning move
        var winningMove = GetPlayerWinningMoveV3();
        if (winningMove != null)
        {
            return winningMove;
        }
        // Check for opponent's winning move
        var opponentWinningMove = GetOpponentWinningMoveV3();
        if (opponentWinningMove != null)
        {
            return opponentWinningMove;
        }

        foreach (var move in moves)
        {
            int oldValue = Board[move.R, move.C];
            Board[move.R, move.C] = Player;
            UpdateZobristHash(move.R, move.C, oldValue, Player);

            int score = Minimax(Board, true, MaxDepth, false, move, int.MinValue, int.MaxValue);

            Board[move.R, move.C] = oldValue;
            UpdateZobristHash(move.R, move.C, Player, oldValue);

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }

        return bestMove;
    }

    private int Minimax(int[,] board, bool isFirstLevelDepth, int depth, bool isMaximizing, Point currentMove, double alpha, double beta)
    {
        // Check transposition table
        if (transpositionTable.TryGetValue(zobristHash, out var entry) &&
            entry.Depth >= depth)
        {
            if (entry.Flag == TranspositionFlag.Exact)
                return entry.Score;
            if (entry.Flag == TranspositionFlag.LowerBound && entry.Score >= beta)
                return entry.Score;
            if (entry.Flag == TranspositionFlag.UpperBound && entry.Score <= alpha)
                return entry.Score;
        }

        if (depth == 0 || IsTerminal(board))
        {
            return EvaluateBoardScoreV3(currentMove, !isMaximizing);
        }

        var radius = isFirstLevelDepth ? 2 : 1;
        var candidateMoves = GetCandidateMoves(board, radius)
            .Select(m => new { Move = m, Score = Heuristic(board, m, isMaximizing) })
            .OrderByDescending(x => x.Score)
            .Take(10)
            .Select(x => x.Move)
            .ToList();

        if (candidateMoves.Count == 0)
        {
            return EvaluateBoardScoreV3(currentMove, !isMaximizing);
        }

        TranspositionFlag flag = TranspositionFlag.UpperBound;
        int bestScore = isMaximizing ? int.MinValue : int.MaxValue;

        foreach (var move in candidateMoves)
        {
            int oldValue = board[move.R, move.C];
            board[move.R, move.C] = isMaximizing ? Player : Opponent;
            UpdateZobristHash(move.R, move.C, oldValue, isMaximizing ? Player : Opponent);

            int score = Minimax(board, false, depth - 1, !isMaximizing, move, alpha, beta);

            board[move.R, move.C] = oldValue;
            UpdateZobristHash(move.R, move.C, isMaximizing ? Player : Opponent, oldValue);

            if (isMaximizing)
            {
                bestScore = Math.Max(bestScore, score);
                alpha = Math.Max(alpha, bestScore);
                if (bestScore >= beta)
                {
                    flag = TranspositionFlag.LowerBound;
                    break;
                }
            }
            else
            {
                bestScore = Math.Min(bestScore, score);
                beta = Math.Min(beta, bestScore);
                if (bestScore <= alpha)
                {
                    flag = TranspositionFlag.UpperBound;
                    break;
                }
            }
        }

        // Determine the flag if not set by cut-off
        if (flag != TranspositionFlag.LowerBound && flag != TranspositionFlag.UpperBound)
        {
            if (bestScore <= alpha)
                flag = TranspositionFlag.UpperBound;
            else if (bestScore >= beta)
                flag = TranspositionFlag.LowerBound;
            else
                flag = TranspositionFlag.Exact;
        }

        transpositionTable[zobristHash] = new TranspositionEntry(bestScore, depth, flag);
        return bestScore;
    }

    private Point? GetPlayerWinningMoveV3() => GetWinningMoveV3(Player, GomokuV3Score.ConfirmWin);
    private Point? GetOpponentWinningMoveV3() => GetWinningMoveV3(Opponent, GomokuV3Score.OpponentConfirmWin);

    private Point? GetWinningMoveV3(int player, int winScore)
    {
        var winningMove = new Point(-1, -1);
        var bestScore = int.MinValue;
        var neighborMoves = GetCandidateMoves(Board);

        foreach (var move in neighborMoves)
        {
            int oldValue = Board[move.R, move.C];
            Board[move.R, move.C] = player;
            UpdateZobristHash(move.R, move.C, oldValue, player);

            int score = EvaluateV3(move);

            Board[move.R, move.C] = oldValue;
            UpdateZobristHash(move.R, move.C, player, oldValue);

            if (score > bestScore)
            {
                bestScore = score;
                winningMove = move;
            }
        }

        return bestScore >= winScore ? winningMove : null;
    }

    private int EvaluateV3(Point move)
    {
        return Heuristic(Board, move);
    }

    private int Heuristic(int[,] board, Point move)
    {
        int player = board[move.R, move.C];
        List<List<int>> listAreaCheckForPlayer = GetListAllDirections(board, move, player);
        var isPlay1Move = player == 1;

        NumberofScorePattern scorePattern = ValuePosition(listAreaCheckForPlayer, isPlay1Move);
        var playerValue = GetScoreByPattern(scorePattern);

        var opponent = 3 - player;
        board[move.R, move.C] = opponent;
        List<List<int>> listAreaCheckForOpponent = GetListAllDirections(board, move, opponent);
        NumberofScorePattern enemyScorePattern = ValuePosition(listAreaCheckForOpponent, !isPlay1Move);
        var enemyValue = GetScoreByPattern(enemyScorePattern);
        board[move.R, move.C] = player;

        return 2 * playerValue - enemyValue;
    }

    private int Heuristic(int[,] board, Point move, bool isPlayerTurn)
    {
        board[move.R, move.C] = isPlayerTurn ? Player : Opponent;
        var score = Heuristic(board, move);
        board[move.R, move.C] = 0; // Reset the move

        return score;
    }

    private static int GetScoreByPattern(NumberofScorePattern scorePattern)
    {
        if (scorePattern.Winning > 0) return GomokuV3Score.WinScore * scorePattern.Winning;
        if (scorePattern.Stone4 > 0) return GomokuV3Score.WinGuarantee;
        if (scorePattern.BlockStone4 > 1) return GomokuV3Score.WinGuaranteBlock;
        if (scorePattern.Stone3 > 0 && scorePattern.Stone4 > 0) return GomokuV3Score.HaveAtLeastOneStone3AndOneStone4;
        if (scorePattern.DoubleStone3WithNoBlock > 0) return GomokuV3Score.HaveAtLeastTwoStone3WithNoBlockWhichInterSectAtOnePoint;
        if (scorePattern.Stone3 > 1) return GomokuV3Score.HaveMoreThanOneStone3;

        if (scorePattern.Stone3 == 1)
        {
            return scorePattern.Stone2 switch
            {
                3 => GomokuV3Score.HaveOneStone3And3Stone2,
                2 => GomokuV3Score.HaveOneStone3And2Stone2,
                1 => GomokuV3Score.HaveOneStone3And1Stone2,
                _ => GomokuV3Score.HaveOnlyOneStone3,
            };
        }

        if (scorePattern.BlockStone4 == 1)
        {
            return scorePattern.Stone2 switch
            {
                3 => GomokuV3Score.HaveOneBlockStone4AndThreeBlockStone2,
                2 => GomokuV3Score.HaveOneBlockStone4AndTwoBlockStone2,
                1 => GomokuV3Score.HaveOneBlockStone4AndOneBlockStone2,
                _ => GomokuV3Score.HaveOnlyOneBlockStone4,
            };
        }

        switch (scorePattern.BlockStone3)
        {
            case 3 when scorePattern.Stone2 == 2:
                return GomokuV3Score.HaveThreeBlockStone3AndOneStone2;
            case 2:
                return scorePattern.Stone2 switch
                {
                    2 => GomokuV3Score.HaveTwoBlockStone3AndTwoStone2,
                    1 => GomokuV3Score.HaveTwoBlockStone3AndOneStone2,
                    _ => 0
                };
            case 1:
                return scorePattern.Stone2 switch
                {
                    3 => GomokuV3Score.HaveOneBlockStone3AndThreeStone2,
                    2 => GomokuV3Score.HaveOneBlockStone3AndTwoStone2,
                    1 => GomokuV3Score.HaveOneBlockStone3AndOneStone2,
                    _ => 0
                };
        }

        return scorePattern.Stone2 switch
        {
            4 => GomokuV3Score.HaveFourBlockStone2,
            3 => GomokuV3Score.HaveThreeBlockStone2,
            2 => GomokuV3Score.HaveTwoBlockStone2,
            1 => GomokuV3Score.HaveOneBlockStone2,
            _ => GomokuV3Score.None,
        };
    }

    private static NumberofScorePattern ValuePosition(List<List<int>> listOfDirections, bool isPlay1Move)
    {
        var scorePattern = new NumberofScorePattern();
        ListStonePattern listPattern = isPlay1Move ? ListStoneForPlayer1 : ListStoneForPlayer2;

        foreach (var list in listOfDirections)
        {
            if (IsAnyInArrays(listPattern.WinPattern, list))
            {
                scorePattern.Winning++;
                continue;
            }
            if (IsAnyInArrays(listPattern.Stone4WithNoBlock, list))
            {
                scorePattern.Stone4++;
                continue;
            }
            if (IsAnyInArrays(listPattern.Stone3WithNoBlock, list))
            {
                scorePattern.Stone3++;
                if (scorePattern.Stone3 >= 2)
                {
                    scorePattern.DoubleStone3WithNoBlock++;
                }
            }
            if (IsAnyInArrays(listPattern.Stone4WithBlock, list))
            {
                scorePattern.BlockStone4++;
                continue;
            }
            if (IsAnyInArrays(listPattern.Stone3WithBlock, list))
            {
                scorePattern.BlockStone3++;
                continue;
            }
            if (IsAnyInArrays(listPattern.Stone2WithNoBlock, list))
            {
                scorePattern.Stone2++;
            }
        }

        return scorePattern;
    }

    private static bool IsAnyInArrays(List<List<int>> listPattern, List<int> listCellValue)
    {
        foreach (var pattern in listPattern)
        {
            int fCount = listCellValue.Count;
            int sCount = pattern.Count;

            for (int i = 0; i <= fCount - sCount; i++)
            {
                int k = 0;
                for (int j = 0; j < sCount; j++)
                {
                    if (listCellValue[i + j] != pattern[j]) break;
                    k++;
                }

                if (k == sCount) return true;
            }
        }

        return false;
    }

    private List<List<int>> GetListAllDirections(int[,] board, Point move, int player)
    {
        int opponent = 3 - player;
        List<List<int>> listAllDirections = [];

        foreach (var (dr, dc) in Directions)
        {
            List<int> listCell = [];

            // Check backward direction
            for (int i = -1; i > -5; i--)
            {
                int rr = move.R + dr * i;
                int cc = move.C + dc * i;

                if (rr < 0 || rr >= Rows || cc < 0 || cc >= Cols) break;

                int cellValue = board[rr, cc];
                listCell.Insert(0, cellValue);
                if (cellValue == opponent)
                {
                    break;
                }
            }

            listCell.Add(player);

            // Check forward direction
            for (int i = 1; i < 5; i++)
            {
                int rr = move.R + dr * i;
                int cc = move.C + dc * i;

                if (rr < 0 || rr >= Rows || cc < 0 || cc >= Cols) break;

                int cellValue = board[rr, cc];

                listCell.Add(cellValue);

                if (cellValue == opponent)
                {
                    break;
                }
            }

            listAllDirections.Add(listCell);
        }

        return listAllDirections;
    }

    private int EvaluateBoardScoreV3(Point move, bool isPlayerTurn)
    {
        return isPlayerTurn ? EvaluateV3(move) : -EvaluateV3(move);
    }

    private static bool IsTerminal(int[,] board)
    {
        return HasWon(board, 1) || HasWon(board, 2) || GetCandidateMoves(board).Count == 0;
    }

    private static List<Point> GetCandidateMoves(int[,] board, int radius = 1)
    {
        HashSet<Point> moves = [];
        int rows = board.GetLength(0);
        int cols = board.GetLength(1);

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (board[r, c] == 0) continue;

                for (int dr = -radius; dr <= radius; dr++)
                {
                    for (int dc = -radius; dc <= radius; dc++)
                    {
                        int nr = r + dr, nc = c + dc;
                        if (nr >= 0 && nr < rows && nc >= 0 && nc < cols && board[nr, nc] == 0)
                        {
                            moves.Add(new Point(nr, nc));
                        }
                    }
                }
            }
        }

        if (moves.Count == 0 && board[rows / 2, cols / 2] == 0)
        {
            moves.Add(new Point(rows / 2, cols / 2));
        }

        return [.. moves];
    }

    private static bool HasWon(int[,] board, int player, bool blockTwoSides = false)
    {
        int size = board.GetLength(0);

        for (int r = 0; r < size; r++)
        {
            for (int c = 0; c < size; c++)
            {
                if (CheckDirection(board, r, c, 0, 1, player, blockTwoSides)) return true;
                if (CheckDirection(board, r, c, 1, 0, player, blockTwoSides)) return true;
                if (CheckDirection(board, r, c, 1, 1, player, blockTwoSides)) return true;
                if (CheckDirection(board, r, c, 1, -1, player, blockTwoSides)) return true;
            }
        }
        return false;
    }

    private static bool CheckDirection(int[,] board, int row, int col, int dr, int dc, int player, bool blockTwoSides)
    {
        if (board[row, col] != player) return false;

        int count = 1;
        for (int i = 1; i < 5; i++)
        {
            int r = row + dr * i;
            int c = col + dc * i;
            if (r >= 0 && c >= 0 && r < board.GetLength(0) && c < board.GetLength(1) && board[r, c] == player)
            {
                count++;
            }
            else break;
        }

        if (count == 5) return true;
        if (blockTwoSides && count == 4) return CheckBlockTwoSides(board, row, col, dr, dc, player);
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

public struct TranspositionEntry
{
    public int Score { get; }
    public int Depth { get; }
    public TranspositionFlag Flag { get; }

    public TranspositionEntry(int score, int depth, TranspositionFlag flag)
    {
        Score = score;
        Depth = depth;
        Flag = flag;
    }
}

public enum TranspositionFlag
{
    Exact,
    LowerBound,
    UpperBound
}

public class GomokuV2Score
{
    public const int Win = 50_000_000;
    public const int Blocked = 0;
    public const int ConfirmWin = 1_000_000;
    public const int Consecutive4_Not_PlayerTurn_NoneBlock = 200_000;
    public const int Consecutive4_Not_PlayerTurn_MoreThanZeroBlock = 300;
    public const int Consecutive3_PlayerTurn_NoneBlock = 50_000;
    public const int Consecutive3_PlayerTurn_MoreThanZeroBlock = 20;
    public const int Consecutive3_Not_PlayerTurn_NoneBlock = 100;
    public const int Consecutive3_Not_PlayerTurn_MoreThanZeroBlock = 5;
    public const int Consecutive2_MoreThanZeroBlock = 3;
    public const int Consecutive2_PlayerTurn_NoneBlock = 7;
    public const int Consecutive2_Not_PlayerTurn_NoneBlock = 5;
    public const int Consecutive1 = 1;
    public const int Invalid = 0;
}

public class GomokuV3Score
{
    public const int WinScore = 1_000_000_000;
    public const int WinGuarantee = WinScore / 10;
    public const int WinGuaranteBlock = WinGuarantee / 10;
    public const int HaveAtLeastTwoStone3WithNoBlockWhichInterSectAtOnePoint = WinGuarantee / 20 + WinGuarantee / 15;
    public const int HaveAtLeastOneStone3AndOneStone4 = WinGuarantee / 100;
    public const int HaveMoreThanOneStone3 = WinGuarantee / 1000;
    public const int HaveOneStone3And3Stone2 = 40_000;
    public const int HaveOneStone3And2Stone2 = 38_000;
    public const int HaveOneStone3And1Stone2 = 35_000;
    public const int HaveOnlyOneStone3 = 3_450;
    public const int HaveOneBlockStone4AndThreeBlockStone2 = 4_500;
    public const int HaveOneBlockStone4AndTwoBlockStone2 = 4_200;
    public const int HaveOneBlockStone4AndOneBlockStone2 = 4_100;
    public const int HaveOnlyOneBlockStone4 = 4_005;
    public const int HaveThreeBlockStone3AndOneStone2 = 2_800;
    public const int HaveTwoBlockStone3AndTwoStone2 = 3_000;
    public const int HaveTwoBlockStone3AndOneStone2 = 2_900;
    public const int HaveOneBlockStone3AndThreeStone2 = 3_400;
    public const int HaveOneBlockStone3AndTwoStone2 = 3_100;
    public const int HaveOneBlockStone3AndOneStone2 = 3_100;
    public const int HaveFourBlockStone2 = 2_700;
    public const int HaveThreeBlockStone2 = 2_500;
    public const int HaveTwoBlockStone2 = 2_000;
    public const int HaveOneBlockStone2 = 1_000;
    public const int None = 0;
    public const int ConfirmWin = WinScore;
    public const int OpponentConfirmWin = WinScore;
}

public class NumberofScorePattern
{
    public int Winning = 0;
    public int Stone4 = 0;
    public int Stone3 = 0;
    public int Stone2 = 0;
    public int BlockStone4 = 0;
    public int BlockStone3 = 0;
    public int DoubleStone3WithNoBlock = 0;

    public bool HasZeroValue => Winning == 0 && Stone4 == 0 && Stone3 == 0 &&
                              Stone2 == 0 && BlockStone4 == 0 && BlockStone3 == 0 && DoubleStone3WithNoBlock == 0;
}

public class ListStonePattern
{
    public List<List<int>> WinPattern = [[1, 1, 1, 1, 1]];
    public List<List<int>> Stone4WithNoBlock = [[0, 1, 1, 1, 1, 0]];

    public List<List<int>> Stone3WithNoBlock =
    [
        [0, 1, 1, 1, 0, 0],
        [0, 0, 1, 1, 1, 0],
        [0, 1, 0, 1, 1, 0],
        [0, 1, 1, 0, 1, 0]
    ];
    // public List<List<int>> Stone3WithNoBlock =
    // [
    //     [0, 1, 1, 1, 0],
    //     [0, 1, 0, 1, 1, 0],
    //     [0, 1, 1, 0, 1, 0]
    // ];

    public List<List<int>> Stone2WithNoBlock =
    [
        [0, 0, 1, 1, 0, 0],
        [0, 1, 0, 1, 0, 0],
        [0, 0, 1, 0, 1, 0],
        [0, 1, 1, 0, 0, 0],
        [0, 0, 0, 1, 1, 0],
        [0, 1, 0, 0, 1, 0]
    ];

    public List<List<int>> Stone4WithBlock =
    [
        [2, 1, 0, 1, 1, 1],
        [2, 1, 1, 0, 1, 1],
        [2, 1, 1, 1, 0, 1],
        [2, 1, 1, 1, 1, 0],
        [0, 1, 1, 1, 1, 2],
        [1, 0, 1, 1, 1, 2],
        [1, 1, 0, 1, 1, 2],
        [1, 1, 1, 0, 1, 2]
    ];

    public List<List<int>> Stone3WithBlock =
    [
        [2, 1, 1, 1, 0, 0],
        [2, 1, 1, 0, 1, 0],
        [2, 1, 0, 1, 1, 0],
        [0, 0, 1, 1, 1, 2],
        [0, 1, 0, 1, 1, 2],
        [0, 1, 1, 0, 1, 2],
        [2, 1, 0, 1, 0, 1, 2],
        [2, 0, 1, 1, 1, 0, 2],
        [2, 1, 1, 0, 0, 1, 2],
        [2, 1, 0, 0, 1, 1, 2]
    ];

    public static ListStonePattern Create(bool isForPlayer1)
    {
        ListStonePattern list = new();
        if (!isForPlayer1)
        {
            list.Stone2WithNoBlock = ConvertListValue(list.Stone2WithNoBlock);
            list.Stone3WithBlock = ConvertListValue(list.Stone3WithBlock);
            list.Stone3WithNoBlock = ConvertListValue(list.Stone3WithNoBlock);
            list.Stone4WithBlock = ConvertListValue(list.Stone4WithBlock);
            list.Stone4WithNoBlock = ConvertListValue(list.Stone4WithNoBlock);
            list.WinPattern = ConvertListValue(list.WinPattern);
        }
        return list;
    }

    private static List<List<int>> ConvertListValue(List<List<int>> listInput)
    {
        for (int i = 0; i < listInput.Count; i++)
        {
            for (int j = 0; j < listInput[i].Count; j++)
            {
                if (listInput[i][j] == 0)
                {
                    continue;
                }
                listInput[i][j] = 3 - listInput[i][j];
            }
        }
        return listInput;
    }
}

// public enum BlockType
// {
//     BothSidesBlock,
//     BlockOneSide,
//     None
// }