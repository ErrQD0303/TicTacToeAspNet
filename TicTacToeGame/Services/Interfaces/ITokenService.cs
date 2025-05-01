using TicTacToeGame.Models;

namespace TicTacToeGame.Services.Interfaces;

public interface ITokenService<T> where T : class
{
    string GenerateAccessToken(T user, int? expiration = null);
    string GenerateRefreshToken(int length = 32);
}