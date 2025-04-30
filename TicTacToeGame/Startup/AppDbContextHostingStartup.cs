using Microsoft.EntityFrameworkCore;
using TicTacToeGame.Data;
using TicTacToeGame.Helpers.Constants;
using TicTacToeGame.Startup;

[assembly: HostingStartup(typeof(AppDbContextHostingStartup))]
namespace TicTacToeGame.Startup;

public class AppDbContextHostingStartup : IHostingStartup
{
    public void Configure(IWebHostBuilder builder)
    {
        builder.ConfigureServices((context, services) =>
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(context.Configuration.GetConnectionString(AppConstants.CurrentConnectionStringSectionName)));
        });
    }
}