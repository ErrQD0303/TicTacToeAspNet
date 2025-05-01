using TicTacToeGame.Models;

namespace TicTacToeGame.Services.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(AppUser user, int? expiration = null);
    string GenerateRefreshToken(int length = 32);
}