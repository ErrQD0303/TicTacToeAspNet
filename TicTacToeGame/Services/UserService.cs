using TicTacToeGame.Helpers;
using TicTacToeGame.Helpers.Mappers;
using TicTacToeGame.Models;
using TicTacToeGame.Models.Dtos.Users;
using TicTacToeGame.Repository;
using TicTacToeGame.Repository.Interfaces;
using TicTacToeGame.Services.Interfaces;

namespace TicTacToeGame.Services;

public class UserService : IUserService
{
    private ILogger<UserService> Logger { get; set; }
    private IUserRepository UserRepository { get; set; }

    public UserService(IUserRepository userRepository, ILogger<UserService> logger)
    {
        Logger = logger;
        UserRepository = userRepository;
    }

    public async Task<LoggedInUserDto?> LoginAsync(string username, string password)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            return null;
        }

        var user = await UserRepository.GetByUsernameAsync(username);

        if (user == null)
        {
            return null;
        }

        return PasswordHelper.VerifyPassword(user.ToHashPasswordUserDto(), password, user.HashedPassword) ?
            user.ToLoggedInUserDto() : null;
    }

    public async Task<bool> RegisterAsync(RegisterUserDto dto)
    {
        if (string.IsNullOrEmpty(dto.Username) || string.IsNullOrEmpty(dto.Password))
        {
            return false;
        }

        if (await UserRepository.GetByUsernameAsync(dto.Username) != null)
        {
            return false;
        }

        var user = dto.ToAppUser();
        user.HashedPassword = PasswordHelper.HashPassword(user.ToHashPasswordUserDto(), dto.Password);

        await UserRepository.AddAsync(user);

        try
        {
            return await UserRepository.SaveChangesAsync() > 0;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error while saving user to the database.");
            return false;
        }
    }

    public async Task<GetUserDto?> GetUserByIdAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return null;
        }

        var user = await UserRepository.GetByIdAsync(id, false);
        return user?.ToGetUserDto();
    }
}