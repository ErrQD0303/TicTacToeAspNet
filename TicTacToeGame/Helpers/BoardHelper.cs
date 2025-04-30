using System.Text.RegularExpressions;
using TicTacToeGame.Helpers.Enum;

namespace TicTacToeGame.Helpers;

public class BoardHelper
{
    public static int BitIndexJump => NearestCeilingExponent(System.Enum.GetValues<CellState>().Length);

    private static int NearestCeilingExponent(int n)
    {
        if (n <= 0)
        {
            throw new ArgumentException("The number must be greater than zero.");
        }

        int exponentNumber = 1;
        int exponent = 2;
        while (n > exponent)
        {
            ++exponentNumber;
            exponent *= 2;
        }
        return exponentNumber;
    }

    public static byte[] EncodeBoard(CellState[,] board)
    {
        int rows = board.GetLength(0); // y
        int cols = board.GetLength(1); // x
        int totalCells = rows * cols;

        int totalBits = totalCells * 2;
        // Trick to always allocate the sufficent byte array, because 1 byte = 8 bits, then if we have 9 bits, 9 / 8 = 1 byte, but wew need 9 bits, so we add 7( which is 1 byte - 1 bit), we will always get enough bytes in the byte array
        // We can also use Math.Ceiling()
        int byteCount = (totalBits + 7) / 8;

        byte[] bytes = new byte[byteCount];

        int bitIndex = 0;
        int bitIndexJump = BitIndexJump;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                var value = (int)board[r, c];
                int byteIndex = bitIndex / 8;
                int offset = bitIndex % 8;

                // Stores 2 bits
                if (offset <= 6)
                {
                    bytes[byteIndex] |= (byte)(value << offset);
                }
                else
                {
                    bytes[byteIndex + 1] |= (byte)(value << (offset % 8));
                }

                bitIndex += bitIndexJump;
            }
        }

        return bytes;
    }

    public static CellState[,] DecodeBoard(byte[] bytes, int rows, int cols)
    {
        CellState[,] board = new CellState[rows, cols];
        int bitIndex = 0;
        int bitIndexJump = BitIndexJump;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                var byteIndex = bitIndex / 8;
                int offset = bitIndex % 8;
                byte byteValue;
                if (offset <= 6)
                {
                    byteValue = bytes[byteIndex];
                }
                else
                {
                    byteValue = bytes[byteIndex + 1];
                }

                var cellValue = byteValue >> (offset % 8);
                board[r, c] = (CellState)(cellValue & 0b11);

                bitIndex += bitIndexJump;
            }
        }

        return board;
    }
}