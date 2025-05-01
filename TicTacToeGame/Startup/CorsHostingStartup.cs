[assembly: HostingStartup(typeof(TicTacToeGame.Startup.CorsHostingStartup))]
namespace TicTacToeGame.Startup;

public class CorsHostingStartup : IHostingStartup
{
    public void Configure(IWebHostBuilder builder)
    {
        builder.ConfigureServices((context, services) =>
        {
            var corsOrigin = Environment.GetEnvironmentVariable("CORS_ORIGIN");
            if (string.IsNullOrEmpty(corsOrigin))
            {
                throw new ArgumentNullException("CORS_ORIGIN environment variable is not set.");
            }
            services.AddCors(options =>
            {
                options.AddPolicy("LocalhostPolicy", policy =>
                {
                    policy.AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials() // Allow credentials for SignalR and other requests
                        .WithOrigins(corsOrigin.Split(',').Select(o => o.Trim()).ToArray()
                        );
                });
            });
        });
    }
}