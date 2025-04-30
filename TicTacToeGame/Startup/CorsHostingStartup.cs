[assembly: HostingStartup(typeof(TicTacToeGame.Startup.CorsHostingStartup))]
namespace TicTacToeGame.Startup;

public class CorsHostingStartup : IHostingStartup
{
    public void Configure(IWebHostBuilder builder)
    {
        builder.ConfigureServices((context, services) =>
        {
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowAnyOrigin();
                });
            });
        });
    }
}