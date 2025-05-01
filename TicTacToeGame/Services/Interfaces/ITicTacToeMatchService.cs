using TicTacToeGame.Models.Dtos.TicTacToeMatches;

namespace TicTacToeGame.Services.Interfaces;

public interface ITicTacToeMatchService
{
    Task<CreatedTicTacToeMatchDto> AddAsync(CreateTicTacToeMatchDto entity);
}