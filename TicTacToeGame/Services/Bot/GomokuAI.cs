using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

public static class Zobrist
{
    private static readonly Random Rand = new Random();
    public static ulong[,,] Table;

    public static void Init(int rows, int cols)
    {
        Table = new ulong[rows, cols, 3];
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                for (int p = 0; p < 3; p++)
                    Table[r, c, p] = ((ulong)Rand.Next() << 32) | (ulong)Rand.Next();
    }

    public static ulong ComputeHash(int[,] board)
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

    public static ulong UpdateHash(ulong hash, int row, int col, int player)
    {
        return hash ^ Table[row, col, player];
    }
}

public class TranspositionTable
{
    private ConcurrentDictionary<ulong, int> table = [];
    public void Store(ulong hash, int value) => table[hash] = value;
    public bool TryGet(ulong hash, out int value) => table.TryGetValue(hash, out value);
}

public class GomokuAI
{
    private int[,] board;
    private int player;
    private Point bestMove;
    private TranspositionTable transTable;
    private ulong currentHash;
    private int rows, cols;

    private static readonly (int dr, int dc)[] Directions = {
        (0, 1), (1, 0), (1, 1), (1, -1)
    };

    public GomokuAI(int[,] board, int player)
    {
        this.board = board;
        this.player = player;
        this.rows = board.GetLength(0);
        this.cols = board.GetLength(1);
        Zobrist.Init(rows, cols);
        this.transTable = new TranspositionTable();
        this.currentHash = Zobrist.ComputeHash(board);
    }

    public Point GetMove()
    {
        var immediate = GetImmediateMove(board, player);
        if (immediate != null)
            return immediate.Value;

        bestMove = new Point(-1, -1);
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

        return bestMove;
    }

    private Point? GetImmediateMove(int[,] b, int currentPlayer)
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

    private bool CheckWin(int[,] b, int r, int c, int player)
    {
        foreach (var (dr, dc) in Directions)
        {
            int count = 1;
            for (int i = 1; i < 5; i++)
            {
                int rr = r + dr * i, cc = c + dc * i;
                if (rr >= 0 && rr < rows && cc >= 0 && cc < cols && b[rr, cc] == player)
                    count++;
                else break;
            }
            for (int i = 1; i < 5; i++)
            {
                int rr = r - dr * i, cc = c - dc * i;
                if (rr >= 0 && rr < rows && cc >= 0 && cc < cols && b[rr, cc] == player)
                    count++;
                else break;
            }
            if (count >= 5) return true;
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

        int lineScore = 0;
        bool openStart = false, openEnd = false;

        int sr = r - dr, sc = c - dc;
        if (sr >= 0 && sr < rows && sc >= 0 && sc < cols && b[sr, sc] == 0) openStart = true;
        if (rr >= 0 && rr < rows && cc >= 0 && cc < cols && b[rr, cc] == 0) openEnd = true;

        if (count >= 5) lineScore = 10000000;
        else if (count == 4 && openStart && openEnd) lineScore = 1000000;
        else if (count == 4 && (openStart || openEnd)) lineScore = 100000;
        else if (count == 3 && openStart && openEnd) lineScore = 10000;
        else if (count == 3 && (openStart || openEnd)) lineScore = 1000;
        else if (count == 2 && openStart && openEnd) lineScore = 100;
        else if (count == 2 && (openStart || openEnd)) lineScore = 10;
        else lineScore = 1;

        return isPlayer ? lineScore : -lineScore;
    }

    private HashSet<Point> GenerateCandidateMoves(int[,] b)
    {
        var moves = new HashSet<Point>();
        int margin = 2;
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                if (b[r, c] != 0)
                    for (int dr = -margin; dr <= margin; dr++)
                        for (int dc = -margin; dc <= margin; dc++)
                        {
                            int rr = r + dr, cc = c + dc;
                            if (rr >= 0 && rr < rows && cc >= 0 && cc < cols && b[rr, cc] == 0)
                                moves.Add(new Point(cc, rr));
                        }

        if (moves.Count == 0 && b[rows / 2, cols / 2] == 0)
            moves.Add(new Point(cols / 2, rows / 2));
        return moves;
    }
}
