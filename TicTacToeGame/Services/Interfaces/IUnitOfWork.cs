using TicTacToeGame.Models;
using TicTacToeGame.Repository.Interfaces;

namespace TicTacToeGame.Services.Interfaces;

public interface IUnitOfWork
{
    ITicTacToeMatchService TicTacToeMatchSerITicTacToeMatchService { get; }
    ITicTacToeMatchHistoryService TicTacToeMatchHistorySerITicTacToeMatchService { get; }
    IUserService UserService { get; }
    ITokenService<SimpleUser> TokenService { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
