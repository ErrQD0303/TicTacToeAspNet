[assembly: HostingStartup(typeof(TicTacToeGame.Startup.SignalRHostingStartup))]
namespace TicTacToeGame.Startup;

public class SignalRHostingStartup : IHostingStartup
{
    public void Configure(IWebHostBuilder builder)
    {
        builder.ConfigureServices((context, services) =>
        {
            services.AddSignalR();
        });
    }
}