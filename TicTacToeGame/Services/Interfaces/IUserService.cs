
using TicTacToeGame.Models.Dtos.Users;

namespace TicTacToeGame.Services.Interfaces;

public interface IUserService
{
    Task<bool> LoginAsync(string username, string password);
    Task<bool> RegisterAsync(RegisterUserDto dto);
}