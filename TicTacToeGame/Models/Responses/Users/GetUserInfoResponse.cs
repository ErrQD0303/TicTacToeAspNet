using TicTacToeGame.Models.Dtos.Users;

namespace TicTacToeGame.Models.Responses.Users;

public class GetUserInfoResponse : Response<GetUserDto>
{

}

public class GetSimpleUserInfoResponse : Response<SimpleUser>
{

}