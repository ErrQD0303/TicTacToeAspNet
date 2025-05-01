
using TicTacToeGame.Models.Dtos.Users;

namespace TicTacToeGame.Services.Interfaces;

public interface IUserService
{
    Task<LoggedInUserDto?> LoginAsync(string username, string password);
    Task<bool> RegisterAsync(RegisterUserDto dto);
    Task<GetUserDto?> GetUserByIdAsync(string id);
}