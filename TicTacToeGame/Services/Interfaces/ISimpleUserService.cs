using TicTacToeGame.Models;

namespace TicTacToeGame.Services.Interfaces
{
    public interface ISimpleUserService
    {
        Task<SimpleUser> LoginAsync(string username);
        Task<SimpleUser?> GetUserByIdAsync(string id);
        Task RemoveAllAsync(string userId);
        Task<SimpleUser?> FirstOrDefaultUserWithNameAsync(string name);
        Task<bool> IsUserStillAvaiable(string userId);
    }
}