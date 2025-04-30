
using TicTacToeGame.Repository;
using TicTacToeGame.Repository.Interfaces;
using TicTacToeGame.Services;
using TicTacToeGame.Services.Interfaces;
using TicTacToeGame.Startup;

[assembly: HostingStartup(typeof(ServiceHostingStartup))]
namespace TicTacToeGame.Startup;

public class ServiceHostingStartup : IHostingStartup
{
    public void Configure(IWebHostBuilder builder)
    {
        builder.ConfigureServices((context, services) =>
        {
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<ITicTacToeMatchRepository, TicTacToeMatchRepository>();
            services.AddTransient<ITicTacToeMatchHistoryRepository, TicTacToeMatchHistoryRepository>();

            services.AddTransient<IUserService, UserService>();
            services.AddTransient<ITicTacToeMatchService, TicTacToeMatchService>();
            services.AddTransient<ITicTacToeMatchHistoryService, TicTacToeMatchHistoryService>();

            services.AddTransient<IUnitOfWork, UnitOfWork>();
        });
    }
}