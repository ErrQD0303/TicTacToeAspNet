using TicTacToeGame.Models;

namespace TicTacToeGame.Repository.Interfaces;

public interface IUserRepository : IRepository<AppUser>
{
    Task<bool> ValidAsync(string username, string hashedPassword);
}