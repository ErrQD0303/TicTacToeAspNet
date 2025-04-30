using TicTacToeGame.Helpers.Constants;
using TicTacToeGame.Hub;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<GameHub>(AppConstants.HubPath.TicTacToeHub);
app.MapControllers();

app.Run();
