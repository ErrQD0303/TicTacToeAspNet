using TicTacToeGame.Models.Dtos.Users;
using TicTacToeGame.Models.Requests.Users;

namespace TicTacToeGame.Helpers.Mappers;

public static class AppMappers
{
    public static RegisterUserDto ToRegisterUserDto(this RegisterUserRequest request)
    {
        return new RegisterUserDto
        {
            Username = request.Username,
            Name = request.Name,
            Password = request.Password,
            Email = request.Email
        };
    }
}