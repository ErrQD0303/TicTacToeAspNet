using TicTacToeGame.Data;
using TicTacToeGame.Services.Interfaces;

namespace TicTacToeGame.Services;

public class UnitOfWork : IUnitOfWork
{
    public ITicTacToeMatchService TicTacToeMatchSerITicTacToeMatchService { get; }
    public ITicTacToeMatchHistoryService TicTacToeMatchHistorySerITicTacToeMatchService { get; }
    public IUserService UserService { get; }
    public ITokenService TokenService { get; }
    private AppDbContext Context { get; set; }

    public UnitOfWork(ITicTacToeMatchService ticTacToeMatchSerITicTacToeMatchService, ITicTacToeMatchHistoryService ticTacToeMatchHistorySerITicTacToeMatchService, IUserService userService, ITokenService tokenService, AppDbContext context)
    {
        TicTacToeMatchSerITicTacToeMatchService = ticTacToeMatchSerITicTacToeMatchService;
        TicTacToeMatchHistorySerITicTacToeMatchService = ticTacToeMatchHistorySerITicTacToeMatchService;
        UserService = userService;
        TokenService = tokenService;
        Context = context;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return Context.SaveChangesAsync(cancellationToken);
    }
}