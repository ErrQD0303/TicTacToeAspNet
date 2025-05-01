using TicTacToeGame.Models;
using TicTacToeGame.Services.Interfaces;

namespace TicTacToeGame.Services;

public class SimpleUserService : ISimpleUserService
{
    protected List<SimpleUser> Users { get; private set; } = [];

    public Task<SimpleUser?> FirstOrDefaultUserWithNameAsync(string name)
    {
        return Task.FromResult(Users.FirstOrDefault(u => u.Name == name) ?? null);
    }

    public Task<List<SimpleUser>> GetAllValidUsers()
    {
        var validUsers = Users.Where(u => u != null && !string.IsNullOrEmpty(u.Name)).ToList();
        return Task.FromResult(validUsers);
    }

    public Task<SimpleUser?> GetUserByIdAsync(string id)
    {
        var user = Users.FirstOrDefault(u => u.Id == id) ?? null;
        return Task.FromResult(user);
    }

    public Task<bool> IsUserStillAvaiable(string userId)
    {
        return Task.FromResult(Users.Any(u => u != null && u.Id == userId && !string.IsNullOrEmpty(u.Name)));
    }

    public Task<SimpleUser> LoginAsync(string id)
    {
        var isUserExists = Users.Any(u => u != null && u.Id == id);
        if (!isUserExists)
        {
            var newUser = new SimpleUser
            {
                Id = id
            };
            Users.Add(newUser);
            return Task.FromResult(newUser);
        }

        var user = Users.FirstOrDefault(u => u.Id == id) ?? throw new Exception("User not found");
        return Task.FromResult(user);
    }

    public Task RemoveAllAsync(string userId)
    {
        Users.RemoveAll(u => u.Id == userId);
        return Task.CompletedTask;
    }
}