using TicTacToeGame.Data;
using TicTacToeGame.Services.Interfaces;

namespace TicTacToeGame.Services;

public class UnitOfWork : IUnitOfWork
{
    public ITicTacToeMatchService TicTacToeMatchSerITicTacToeMatchService { get; }
    public ITicTacToeMatchHistoryService TicTacToeMatchHistorySerITicTacToeMatchService { get; }
    public IUserService UserService { get; }
    private AppDbContext Context { get; set; }

    public UnitOfWork(ITicTacToeMatchService ticTacToeMatchSerITicTacToeMatchService, ITicTacToeMatchHistoryService ticTacToeMatchHistorySerITicTacToeMatchService, IUserService userService, AppDbContext context)
    {
        TicTacToeMatchSerITicTacToeMatchService = ticTacToeMatchSerITicTacToeMatchService;
        TicTacToeMatchHistorySerITicTacToeMatchService = ticTacToeMatchHistorySerITicTacToeMatchService;
        UserService = userService;
        Context = context;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return Context.SaveChangesAsync(cancellationToken);
    }
}