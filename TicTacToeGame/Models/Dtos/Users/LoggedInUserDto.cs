namespace TicTacToeGame.Models.Dtos.Users;

public class LoggedInUserDto
{
    public string Id { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
}