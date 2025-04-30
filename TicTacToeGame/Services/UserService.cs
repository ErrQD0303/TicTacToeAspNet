using TicTacToeGame.Models.Dtos.Users;
using TicTacToeGame.Repository;
using TicTacToeGame.Services.Interfaces;

namespace TicTacToeGame.Services;

public class UserService : IUserService
{
    private UserRepository UserRepository { get; set; }

    public UserService(UserRepository userRepository)
    {
        UserRepository = userRepository;
    }

    public Task<bool> LoginAsync(string username, string password)
    {
        throw new NotImplementedException();
    }

    public Task<bool> RegisterAsync(RegisterUserDto dto)
    {
        throw new NotImplementedException();
    }
}