using TicTacToeGame.Models;

namespace TicTacToeGame.Repository.Interfaces;

public interface IUserRepository : IRepository<AppUser>
{
    Task<AppUser?> GetByUsernameAsync(string username, bool trackChanges = false);
    Task<bool> ValidAsync(string username, string hashedPassword);
}