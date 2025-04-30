using TicTacToeGame.Helpers.Constants;
using TicTacToeGame.Helpers.Extensions;
using TicTacToeGame.Startup;

[assembly: HostingStartup(typeof(ControllerHostingStartup))]
namespace TicTacToeGame.Startup;

public class ControllerHostingStartup : IHostingStartup
{
    public void Configure(IWebHostBuilder builder)
    {
        builder.ConfigureServices((context, services) =>
        {
            services.AddControllers(o =>
            {
                o.UseRoutePrefix(AppConstants.DefaultRoutePrefix);
            });
        });
    }
}