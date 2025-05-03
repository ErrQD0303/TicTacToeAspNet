using TicTacToeGame.Services;

namespace TicTacToeGame.Helpers;

public class BotNameHelper
{
    private static readonly Random _random = new();
    public static string GetBotName(BotType botType = BotType.Gomoku)
    {
        var number = _random.Next(0, 100000); // Just to make sure the random number is generated
        return (botType switch
        {
            BotType.TicTacToe => "SimpleBot",
            BotType.Gomoku => "GomokuBot",
            _ => throw new ArgumentOutOfRangeException(nameof(botType), botType, null)
        }) + number;
    }
}