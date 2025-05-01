using TicTacToeGame.Helpers;
using TicTacToeGame.Helpers.Enum;

namespace TicTacToeGame.Models;

public class TicTacToeMatch
{
    private int _row = 3;
    private int _column = 3;
    public string Id { get; set; } = default!;
    public int Row
    {
        get => _row; set
        {
            if (value < 3)
            {
                throw new ArgumentOutOfRangeException(nameof(Row), "Row must be between 3 and 10.");
            }
            _row = value;
            Board = new CellState[_row, _column];
        }
    }

    public int Column
    {
        get => _column; set
        {
            if (value < 3)
            {
                throw new ArgumentOutOfRangeException(nameof(Column), "Column must be between 3 and 10.");
            }
            _column = value;
            Board = new CellState[_row, _column];
        }
    }

    public virtual CellState[,] Board
    {
        get
        {
            return BoardHelper.DecodeBoard(BoardData, Row, Column);
        }
        set
        {
            BoardData = BoardHelper.EncodeBoard(value);
        }
    }
    public virtual byte[] BoardData
    {
        get; protected set;
    }
    public virtual string? Player1Id { get; set; } = default!;
    public virtual string? Player2Id { get; set; } = default!;
    public virtual bool IsPlayer1Turn { get; set; } = true;

    // Navigation properties
    public virtual TicTacToeMatchHistory TicTacToeMatchHistory { get; set; } = default!;
    public virtual AppUser? Player1 { get; set; } = default!;
    public virtual AppUser? Player2 { get; set; } = default!;

    public TicTacToeMatch(int row, int column)
    {
        Row = row;
        Column = column;
        // Default constructor initializes the board with default size
        BoardData = Array.Empty<byte>();
        Board = new CellState[Row, Column];
    }

    public TicTacToeMatch() : this(3, 3)
    {
    }
}