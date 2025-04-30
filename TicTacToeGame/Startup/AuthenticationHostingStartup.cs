using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TicTacToeGame.Helpers.Constants;
using TicTacToeGame.Helpers.Options;
using TicTacToeGame.Startup;

[assembly: HostingStartup(typeof(AuthenticationHostingStartup))]
namespace TicTacToeGame.Startup;

public class AuthenticationHostingStartup : IHostingStartup
{
    public void Configure(IWebHostBuilder builder)
    {
        builder.ConfigureServices((context, services) =>
        {
            services.Configure<JwtConfigurationOptions>(context.Configuration.GetSection("Jwt"));

            var jwtOptions = context.Configuration.GetSection("Jwt").Get<JwtConfigurationOptions>() ?? throw new ArgumentNullException("Jwt options cannot be null");

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = jwtOptions.Issuer;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.IssuerSigningKey)),
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments(AppConstants.HubPath.BasePath))
                        {
                            // Read the token out of the query string
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });
        });
    }
}