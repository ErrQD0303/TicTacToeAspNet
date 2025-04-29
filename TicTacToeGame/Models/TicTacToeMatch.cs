using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicTacToeGame.Models;

public class TicTacToeMatch
{
    public static string Player1Mark => "X";
    public static string Player2Mark => "O";
    public string Player1 { get; set; } = default!;
    public string Player2 { get; set; } = default!;
    public bool IsPlayer1Turn { get; set; } = true;
}