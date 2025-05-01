using NSwag;
using NSwag.Generation.Processors.Security;

[assembly: HostingStartup(typeof(TicTacToeGame.Startup.SwaggerHostingStartup))]
namespace TicTacToeGame.Startup;

public class SwaggerHostingStartup : IHostingStartup
{
    public void Configure(IWebHostBuilder builder)
    {
        builder.ConfigureServices((context, services) =>
        {
            services.AddEndpointsApiExplorer();

            services.AddOpenApiDocument(options =>
            {
                options.Title = "TicTacToe Backend";
                options.Version = "v1";
                options.Description = "TicTacToe Backend API";
                options.DocumentName = "v1";

                options.AddSecurity("JWT", new NSwag.OpenApiSecurityScheme
                {
                    Type = OpenApiSecuritySchemeType.ApiKey,
                    Name = Microsoft.Net.Http.Headers.HeaderNames.Authorization,
                    In = OpenApiSecurityApiKeyLocation.Header,
                    Description = "Type into the textbox: Bearer {your JWT token}.",
                    BearerFormat = "JWT",
                    Scheme = "bearer",
                });

                options.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("JWT"));
            });
        });
    }
}