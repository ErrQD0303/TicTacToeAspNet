using TicTacToeGame.Helpers.Enum;
using TicTacToeGame.Models;
using TicTacToeGame.Models.Dtos.TicTacToeMatches;
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

    public static LoggedInUserDto ToLoggedInUserDto(this AppUser user)
    {
        return new LoggedInUserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Name = user.Name,
            Email = user.Email
        };
    }

    public static AppUser ToAppUser(this RegisterUserDto dto)
    {
        return new AppUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = dto.Username,
            Name = dto.Name,
            Email = dto.Email,
            HashedPassword = string.Empty // Password will be hashed later
        };
    }

    public static AppUser ToAppUser(this LoggedInUserDto dto)
    {
        return new AppUser
        {
            Id = dto.Id,
            UserName = dto.UserName,
            Name = dto.Name,
            Email = dto.Email,
            HashedPassword = string.Empty
        };
    }

    public static HashPasswordUserDto ToHashPasswordUserDto(this AppUser user)
    {
        return new HashPasswordUserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Name = user.Name,
            Email = user.Email
        };
    }

    public static GetUserDto ToGetUserDto(this AppUser user)
    {
        return new GetUserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Name = user.Name,
            Email = user.Email
        };
    }

    public static AppUser ToAppUser(this GetUserDto dto)
    {
        return new AppUser
        {
            Id = dto.Id,
            UserName = dto.UserName,
            Name = dto.Name,
            Email = dto.Email
        };
    }

    public static TicTacToeMatch ToAppUser(this CreateTicTacToeMatchDto dto)
    {
        return new TicTacToeMatch
        {
            Id = dto.Id,
            Row = dto.Row,
            Column = dto.Column,
            Board = dto.Board,
            Player1Id = dto.Player1Id,
            Player2Id = dto.Player2Id,
            IsPlayer1Turn = dto.IsPlayer1Turn,
            TicTacToeMatchHistory = new TicTacToeMatchHistory
            {
                TicTacToeMatchId = dto.Id,
                Result = dto.Result,
                CreatedAt = DateTime.UtcNow,
            }
        };
    }

    public static CreatedTicTacToeMatchDto ToCreatedTicTacToeMatchDto(this TicTacToeMatch match)
    {
        return new CreatedTicTacToeMatchDto
        {
            Id = match.Id,
            Row = match.Row,
            Column = match.Column,
            Board = match.Board,
            Player1Id = match.Player1Id,
            Player2Id = match.Player2Id,
            IsPlayer1Turn = match.IsPlayer1Turn,
            TicTacToeMatchHistory = new CreatedTicTacToeMatchHistoryDto
            {
                Result = match.TicTacToeMatchHistory.Result,
                CreatedAt = match.TicTacToeMatchHistory.CreatedAt,
            }
        };
    }

    public static CreateTicTacToeMatchDto ToCreateTicTacToeMatchDto(this TicTacToeMatch match)
    {
        return new CreateTicTacToeMatchDto
        {
            Id = match.Id,
            Row = match.Row,
            Column = match.Column,
            Board = match.Board,
            Player1Id = match.Player1Id,
            Player2Id = match.Player2Id,
            IsPlayer1Turn = match.IsPlayer1Turn,
            Result = match.TicTacToeMatchHistory.Result,
            CreatedAt = match.TicTacToeMatchHistory.CreatedAt,
        };
    }
}