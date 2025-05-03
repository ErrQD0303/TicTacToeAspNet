
using TicTacToeGame.Models;
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
            services.AddSingleton<ISimpleUserService, SimpleUserService>();
            services.AddSingleton<ITokenService<SimpleUser>, SimpleTokenService>();
            services.AddTransient<IBotService, BotService>();
        });
    }
}