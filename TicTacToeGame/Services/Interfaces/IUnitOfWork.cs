using TicTacToeGame.Repository.Interfaces;

namespace TicTacToeGame.Services.Interfaces;

public interface IUnitOfWork
{
    ITicTacToeMatchService TicTacToeMatchSerITicTacToeMatchService { get; }
    ITicTacToeMatchHistoryService TicTacToeMatchHistorySerITicTacToeMatchService { get; }
    IUserService UserService { get; }
    ITokenService TokenService { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
