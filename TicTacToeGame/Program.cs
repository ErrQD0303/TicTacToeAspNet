using TicTacToeGame.Helpers.Constants;
using TicTacToeGame.Hub;
using DotNetEnv;

Env.Load();
var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi(configure =>
    {
        configure.DocumentTitle = "TicTacToe Backend API";
        configure.Path = "/swagger";
        configure.DocumentPath = "/swagger/{documentName}/swagger.json";
        configure.DocExpansion = "list";
    });
}

app.UseCors("LocalhostPolicy");
app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<GameHub>(AppConstants.HubPath.TicTacToeHub);
app.MapControllers();

app.Run();
