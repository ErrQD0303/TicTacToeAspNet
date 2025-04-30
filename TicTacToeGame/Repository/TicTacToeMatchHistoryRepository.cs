using Microsoft.EntityFrameworkCore;
using TicTacToeGame.Data;
using TicTacToeGame.Models;
using TicTacToeGame.Repository.Interfaces;

namespace TicTacToeGame.Repository;

public class TicTacToeMatchHistoryRepository : ITicTacToeMatchHistoryRepository
{

    public AppDbContext Context { get; set; }

    public TicTacToeMatchHistoryRepository(AppDbContext context)
    {
        Context = context;
    }

    public Task AddAsync(TicTacToeMatchHistory entity)
    {
        return Task.FromResult(Context.Add(entity));
    }

    public Task DeleteAsync(TicTacToeMatchHistory entity)
    {
        Context.Remove(entity);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<TicTacToeMatchHistory>> GetAllAsync(int page = 0, int limit = 0, string? search = null, bool trackChanges = false)
    {
        if (page < 0 || limit < 0)
        {
            throw new ArgumentOutOfRangeException("Page and limit must be non-negative.");
        }

        var query = GetQuery(trackChanges);

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(u => u.TicTacToeMatch.Player1 != null && u.TicTacToeMatch.Player1.UserName.Contains(search) || u.TicTacToeMatch.Player2 != null && u.TicTacToeMatch.Player2.UserName.Contains(search));
        }

        if (limit > 0)
        {
            query = query.Skip(page * limit).Take(limit);
        }

        return Task.FromResult(query.ToList().AsEnumerable());
    }

    public Task<TicTacToeMatchHistory?> GetByIdAsync(string id, bool trackChanges = false)
    {
        return Task.FromResult(GetQuery(trackChanges)
            .FirstOrDefault(u => u.TicTacToeMatchId == id));
    }

    public Task UpdateAsync(TicTacToeMatchHistory entity)
    {
        Context.Update(entity);
        return Task.CompletedTask;
    }

    private IQueryable<TicTacToeMatchHistory> GetQuery(bool trackChanges = false)
    {
        var query = Context.TicTacToeMatchHistories
            .AsQueryable();

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        query = query
            .Include(u => u.TicTacToeMatch)
                .ThenInclude(m => m.Player1)
            .Include(u => u.TicTacToeMatch)
                .ThenInclude(m => m.Player2);

        return query;
    }

    public Task<bool> ExistsAsync(string id)
    {
        return Task.FromResult(Context.TicTacToeMatchHistories.Any(u => u.TicTacToeMatchId == id));
    }
}