using System.Collections.Concurrent;

namespace TicTacToeGame.Services.Bot;

using System.Collections.Concurrent;
using System.Collections;

public class Zobrist
{
    private ulong[,,] _table;

    public ulong[,,] Table => _table ?? throw new InvalidOperationException("Zobrist table not initialized");

    private static readonly Random Rand = new();

    private Zobrist() { }

    public static Zobrist Create(int rows, int cols)
    {
        var zobrist = new Zobrist { _table = new ulong[rows, cols, 3] };
        var usedHashes = new HashSet<ulong>();

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                for (int p = 0; p < 3; p++)
                {
                    ulong num;
                    do
                    {
                        num = ((ulong)(uint)Rand.Next() << 32) | (uint)Rand.Next();
                    } while (usedHashes.Contains(num));

                    zobrist._table[r, c, p] = num;
                    usedHashes.Add(num);
                }
            }
        }

        return zobrist;
    }

    public ulong ComputeHash(int[,] board)
    {
        ulong hash = 0;
        int rows = board.GetLength(0), cols = board.GetLength(1);
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                if (board[r, c] != 0)
                    hash ^= Table[r, c, board[r, c]];
        return hash;
    }

    public ulong UpdateHash(ulong hash, int row, int col, int player)
        => hash ^ Table[row, col, player];
}

public class TranspositionTable
{
    private static ConcurrentDictionary<(ulong hash, int depth), int> _table = new();

    public void Store(ulong hash, int depth, int value) => _table[(hash, depth)] = value;

    public bool TryGet(ulong hash, int minDepth, out int value)
    {
        // Check exact depth first
        if (_table.TryGetValue((hash, minDepth), out value))
            return true;

        // Search for higher depths
        int highestDepth = -1;
        int bestValue = 0;
        bool found = false;

        foreach (var kv in _table)
        {
            if (kv.Key.hash == hash && kv.Key.depth >= minDepth && kv.Key.depth > highestDepth)
            {
                highestDepth = kv.Key.depth;
                bestValue = kv.Value;
                found = true;
            }
        }

        value = bestValue;
        return found;
    }

    public void Clear() => _table.Clear();
}

public class GomokuAI
{
    private const int MAX_SIZE = 25;
    private readonly int[,] _board;
    private readonly int _player;
    private Point _bestMove;
    private static readonly Zobrist Zobrist = Zobrist.Create(MAX_SIZE, MAX_SIZE);
    private readonly ulong _currentHash;
    private readonly int _rows, _cols;
    private readonly bool _blockTwoSides;
    private readonly int _maxDepth;
    private readonly TranspositionTable _transpositionTable = new();

    private static readonly (int dr, int dc)[] Directions = {
            (0, 1), (1, 0), (1, 1), (1, -1)
        };

    public GomokuAI(int[,] board, int player, bool blockTwoSides = false, int depth = 5)
    {
        _board = (int[,])board.Clone();
        _player = player;
        _rows = board.GetLength(0);
        _cols = board.GetLength(1);
        _blockTwoSides = blockTwoSides;
        _maxDepth = depth;
        _currentHash = Zobrist.ComputeHash(_board);
    }

    private int CalculateDynamicDepth(int moveNumber)
    {
        if (moveNumber < 4) return 2;
        if (moveNumber < 8) return 3;
        if (moveNumber < 12) return 4;
        return _maxDepth;
    }

    public Point GetMove()
    {
        int depth = CalculateDynamicDepth(CountNonEmptyCell());
        // Check for immediate wins/blocks
        var immediateMove = GetImmediateMove(out var candidateMoves);
        if (immediateMove != null)
            return immediateMove;

        _bestMove = new Point(-1, -1);
        int bestScore = int.MinValue;

        // Generate and order moves
        var moves = candidateMoves.ToList()
            .OrderByDescending(p => MoveHeuristic(p.R, p.C))
            .ToList();

        foreach (var move in moves)
        {
            _board[move.R, move.C] = _player;
            ulong newHash = Zobrist.UpdateHash(_currentHash, move.R, move.C, _player);
            int score = Math.Abs(Minimax(depth, false, int.MinValue, int.MaxValue, newHash, move));
            _board[move.R, move.C] = 0; // Undo move
            System.Console.WriteLine($"Move: {move.R},{move.C}: {score}");

            if (score > bestScore)
            {
                bestScore = score;
                _bestMove = move;
            }
        }

        return _bestMove;
    }

    private int Minimax(int depth, bool maximizingPlayer, int alpha, int beta, ulong hash, Point cell)
    {
        // // Check transposition table
        // if (_transpositionTable.TryGet(hash, depth, out int cached))
        //     return cached;

        // Terminal conditions
        if (depth == 0)
            return EvaluateBoard();

        // Generate and order moves
        var moves = GenerateCellCandidateMoves(cell).ToList()
            .OrderByDescending(p => MoveHeuristic(p.R, p.C))
            .ToList();

        if (moves.Count == 0)
            return EvaluateBoard();

        int bestValue = maximizingPlayer ? int.MinValue : int.MaxValue;

        foreach (var move in moves)
        {
            int player = maximizingPlayer ? _player : 3 - _player;
            _board[move.R, move.C] = player;
            ulong newHash = Zobrist.UpdateHash(hash, move.R, move.C, player);

            int value = Minimax(depth - 1, !maximizingPlayer, alpha, beta, newHash, move);

            _board[move.R, move.C] = 0; // Undo move

            if (maximizingPlayer)
            {
                bestValue = Math.Max(bestValue, value);
                alpha = Math.Max(alpha, value);
                if (alpha >= beta)
                    break;
            }
            else
            {
                bestValue = Math.Min(bestValue, value);
                beta = Math.Min(beta, value);
                if (beta <= alpha)
                    break;
            }
        }

        _transpositionTable.Store(hash, depth, bestValue);
        return bestValue;
    }

    private int CountNonEmptyCell()
    {
        int count = 0;
        for (int r = 0; r < _rows; r++)
        {
            for (int c = 0; c < _cols; c++)
            {
                if (_board[r, c] != 0)
                {
                    count++;
                }
            }
        }

        return count;
    }

    private Point? GetImmediateMove(out HashSet<Point> candidateMoves)
    {
        candidateMoves = GenerateCandidateMoves();
        foreach (var move in candidateMoves)
        {
            // Check if we can win immediately
            _board[move.R, move.C] = _player;
            if (CheckWin(move.R, move.C, _player))
            {
                _board[move.R, move.C] = 0;
                return move;
            }
            _board[move.R, move.C] = 0;

            // Check if opponent can win next move
            _board[move.R, move.C] = 3 - _player;
            if (CheckWin(move.R, move.C, 3 - _player))
            {
                _board[move.R, move.C] = 0;
                return move;
            }
            _board[move.R, move.C] = 0;
        }

        return null;
    }

    private bool CheckWin(int row, int col, int player)
    {
        foreach (var (dr, dc) in Directions)
        {
            int count = 1;

            // Check positive direction
            for (int i = 1; i <= 4; i++)
            {
                int r = row + dr * i;
                int c = col + dc * i;
                if (r < 0 || r >= _rows || c < 0 || c >= _cols || _board[r, c] != player)
                    break;
                count++;
            }

            // Check negative direction
            for (int i = 1; i <= 4; i++)
            {
                int r = row - dr * i;
                int c = col - dc * i;
                if (r < 0 || r >= _rows || c < 0 || c >= _cols || _board[r, c] != player)
                    break;
                count++;
            }

            if (count >= 5)
            {
                if (!_blockTwoSides) return true;

                // Check open ends for blocked version
                int endR = row + dr * (count - 1);
                int endC = col + dc * (count - 1);
                bool positiveOpen = endR + dr >= 0 && endR + dr < _rows &&
                                  endC + dc >= 0 && endC + dc < _cols &&
                                  _board[endR + dr, endC + dc] == 0;

                int startR = row - dr * (count - 1);
                int startC = col - dc * (count - 1);
                bool negativeOpen = startR - dr >= 0 && startR - dr < _rows &&
                                   startC - dc >= 0 && startC - dc < _cols &&
                                   _board[startR - dr, startC - dc] == 0;

                if (positiveOpen || negativeOpen)
                    return true;
            }
        }
        return false;
    }

    private int MoveHeuristic(int r, int c)
    {
        int score = 0;
        int opponent = 3 - _player;

        // Base score for position (center is better)
        score += 10 - (Math.Abs(r - _rows / 2) + Math.Abs(c - _cols / 2));

        foreach (var (dr, dc) in Directions)
        {
            int playerCount = 0, opponentCount = 0;
            int playerPotential = 0, opponentPotential = 0;

            // Evaluate both directions
            for (int i = -1; i <= 1; i += 2)
            {
                int rr = r + dr * i, cc = c + dc * i;
                bool playerBlocked = false, opponentBlocked = false;

                while (rr >= 0 && rr < _rows && cc >= 0 && cc < _cols)
                {
                    if (!playerBlocked)
                    {
                        if (_board[rr, cc] == _player)
                            playerCount++;
                        else if (_board[rr, cc] == opponent)
                            playerBlocked = true;
                        else
                            playerPotential++;
                    }

                    if (!opponentBlocked)
                    {
                        if (_board[rr, cc] == opponent)
                            opponentCount++;
                        else if (_board[rr, cc] == _player)
                            opponentBlocked = true;
                        else
                            opponentPotential++;
                    }

                    rr += dr * i;
                    cc += dc * i;
                }
            }

            // Score based on actual pieces and potential
            score += (int)Math.Pow(playerCount, 3) + playerPotential;
            score -= (int)Math.Pow(opponentCount, 3) + opponentPotential;
        }

        return score;
    }

    private int EvaluateBoard()
    {
        int score = 0;
        var evaluated = new BitArray(_rows * _cols * Directions.Length);

        for (int r = 0; r < _rows; r++)
        {
            for (int c = 0; c < _cols; c++)
            {
                if (_board[r, c] == 0) continue;

                for (int dir = 0; dir < Directions.Length; dir++)
                {
                    int index = (r * _cols + c) * Directions.Length + dir;
                    if (evaluated[index]) continue;

                    var (dr, dc) = Directions[dir];
                    int rr = r, cc = c;

                    while (rr >= 0 && rr < _rows && cc >= 0 && cc < _cols && _board[rr, cc] == _board[r, c])
                    {
                        int currIndex = (rr * _cols + cc) * Directions.Length + dir;
                        evaluated[currIndex] = true;
                        rr += dr;
                        cc += dc;
                    }

                    score += EvaluateLine(r, c, dr, dc, _board[r, c] == _player);
                }
            }
        }

        return score;
    }

    private int EvaluateLine(int r, int c, int dr, int dc, bool isPlayer)
    {
        int cellOwner = isPlayer ? _player : 3 - _player;
        int count = 1;
        int openEnds = 0;
        bool blockedPositive = false, blockedNegative = false;

        // Check positive direction
        int rr = r + dr, cc = c + dc;
        while (rr >= 0 && rr < _rows && cc >= 0 && cc < _cols &&
              (_board[rr, cc] == cellOwner || (!blockedPositive && _board[rr, cc] == 0)))
        {
            if (_board[rr, cc] == 0)
            {
                openEnds++;
                blockedPositive = true;
            }
            else
            {
                count++;
            }
            rr += dr;
            cc += dc;
        }

        // Check negative direction
        rr = r - dr;
        cc = c - dc;
        while (rr >= 0 && rr < _rows && cc >= 0 && cc < _cols &&
              (_board[rr, cc] == cellOwner || (!blockedNegative && _board[rr, cc] == 0)))
        {
            if (_board[rr, cc] == 0)
            {
                openEnds++;
                blockedNegative = true;
            }
            else
            {
                count++;
            }
            rr -= dr;
            cc -= dc;
        }

        // Improved scoring with better differentiation
        int lineScore = 0;

        if (count >= 5)
        {
            lineScore = 1_000_000; // Immediate win
        }
        else if (count == 4)
        {
            if (openEnds >= 1) lineScore = 700_000; // 4 with at least one open end
            else lineScore = 1_000; // Blocked four
        }
        else if (count == 3)
        {
            if (openEnds >= 2) lineScore = 500_000; // Open three
            else if (openEnds == 1) lineScore = 10_000; // Semi-open three
            else lineScore = 100; // Blocked three
        }
        else if (count == 2)
        {
            if (openEnds >= 2) lineScore = 2_000; // Open two
            else if (openEnds == 1) lineScore = 200; // Semi-open two
            else lineScore = 10; // Blocked two
        }
        else if (count == 1 && openEnds >= 1)
        {
            lineScore = 5; // Potential starting point
        }

        // Add small bonus for lines that can potentially grow longer
        if (count >= 2 && openEnds >= 1)
        {
            lineScore += lineScore / 2;
        }

        return isPlayer ? lineScore : -lineScore;
    }

    private HashSet<Point> GenerateCellCandidateMoves(Point cell)
    {
        var moves = new HashSet<Point>();
        int margin = 1;

        foreach (var (dr, dc) in Directions)
        {
            for (int i = -margin; i <= margin; i++)
            {
                int rr = cell.R + dr * i,
                    cc = cell.C + dc * i;

                if (rr >= 0 && rr < _rows && cc >= 0 && cc < _cols && _board[rr, cc] == 0)
                {
                    moves.Add(new(rr, cc));
                }
            }
        }

        return moves;
    }

    private HashSet<Point> GenerateCandidateMoves()
    {
        var moves = new HashSet<Point>();

        // Find empty spots near occupied positions
        for (int r = 0; r < _rows; r++)
        {
            for (int c = 0; c < _cols; c++)
            {
                if (_board[r, c] != 0)
                {
                    foreach (var (dr, dc) in Directions)
                    {
                        for (int i = -2; i <= 2; i++)
                        {
                            if (i == 0)
                            {
                                continue;
                            }
                            int rr = r + dr * i;
                            int cc = c + dc * i;
                            if (rr >= 0 && rr < _rows && cc >= 0 && cc < _cols && _board[rr, cc] == 0 && moves.All(p => p.R != rr || p.C != cc))
                                moves.Add(new Point(rr, cc));
                        }
                    }
                }
            }
        }


        // Default center move if no candidates
        if (moves.Count == 0 && _board[_rows / 2, _cols / 2] == 0)
            moves.Add(new Point(_rows / 2, _cols / 2));

        return moves;
    }
}