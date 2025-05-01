using Microsoft.EntityFrameworkCore;
using TicTacToeGame.Data;
using TicTacToeGame.Models;
using TicTacToeGame.Repository.Interfaces;

namespace TicTacToeGame.Repository;

public class TicTacToeMatchRepository : ITicTacToeMatchRepository
{

    public AppDbContext Context { get; set; }

    public TicTacToeMatchRepository(AppDbContext context)
    {
        Context = context;
    }

    public async Task AddAsync(TicTacToeMatch entity)
    {
        await Context.AddAsync(entity);
        await Context.AddAsync(entity.TicTacToeMatchHistory);
    }

    public Task DeleteAsync(TicTacToeMatch entity)
    {
        Context.Remove(entity);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<TicTacToeMatch>> GetAllAsync(int page = 0, int limit = 0, string? search = null, bool trackChanges = false)
    {
        if (page < 0 || limit < 0)
        {
            throw new ArgumentOutOfRangeException("Page and limit must be non-negative.");
        }

        var query = GetQuery(trackChanges);

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(u => u.Player1 != null && u.Player1.UserName.Contains(search) || u.Player2 != null && u.Player2.UserName.Contains(search));
        }

        if (limit > 0)
        {
            query = query.Skip(page * limit).Take(limit);
        }

        return Task.FromResult(query.ToList().AsEnumerable());
    }

    public Task<TicTacToeMatch?> GetByIdAsync(string id, bool trackChanges = false)
    {
        return Task.FromResult(GetQuery(trackChanges)
            .FirstOrDefault(u => u.Id == id));
    }

    public Task UpdateAsync(TicTacToeMatch entity)
    {
        Context.Update(entity);
        return Task.CompletedTask;
    }

    private IQueryable<TicTacToeMatch> GetQuery(bool trackChanges = false)
    {
        var query = Context.TicTacToeMatches
            .AsQueryable();

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        query = query
            .Include(m => m.Player1)
            .Include(m => m.Player2)
            .Include(m => m.TicTacToeMatchHistory);

        return query;
    }

    public Task<bool> ExistsAsync(string id)
    {
        return Task.FromResult(Context.TicTacToeMatches.Any(u => u.Id == id));
    }

    public Task<int> SaveChangesAsync()
    {
        return Context.SaveChangesAsync();
    }
}