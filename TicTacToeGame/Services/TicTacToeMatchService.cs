using TicTacToeGame.Helpers.Mappers;
using TicTacToeGame.Models.Dtos.TicTacToeMatches;
using TicTacToeGame.Repository.Interfaces;
using TicTacToeGame.Services.Interfaces;

namespace TicTacToeGame.Services;

public class TicTacToeMatchService : ITicTacToeMatchService
{
    private ITicTacToeMatchRepository TicTacToeMatchRepository { get; set; }

    public TicTacToeMatchService(ITicTacToeMatchRepository ticTacToeMatchRepository)
    {
        TicTacToeMatchRepository = ticTacToeMatchRepository;
    }

    public async Task<CreatedTicTacToeMatchDto> AddAsync(CreateTicTacToeMatchDto entity)
    {
        await TicTacToeMatchRepository.AddAsync(entity.ToAppUser());

        try
        {
            var successful = await TicTacToeMatchRepository.SaveChangesAsync() > 0;
            var createdEntity = await TicTacToeMatchRepository.GetByIdAsync(entity.Id);
            if (createdEntity == null || !successful)
            {
                throw new Exception("Error creating TicTacToeMatch");
            }
            return createdEntity.ToCreatedTicTacToeMatchDto();
        }
        catch (Exception ex)
        {
            throw new Exception("Error saving changes to the database", ex);
        }
    }
}