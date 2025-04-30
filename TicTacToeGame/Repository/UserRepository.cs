using Microsoft.EntityFrameworkCore;
using TicTacToeGame.Data;
using TicTacToeGame.Models;
using TicTacToeGame.Repository.Interfaces;

namespace TicTacToeGame.Repository;

public class UserRepository : IUserRepository
{
    public AppDbContext Context { get; set; }

    public UserRepository(AppDbContext context)
    {
        Context = context;
    }

    public Task AddAsync(AppUser entity)
    {
        return Task.FromResult(Context.Add(entity));
    }

    public Task DeleteAsync(AppUser user)
    {
        Context.Remove(user);
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<AppUser>> GetAllAsync(int page = 0, int limit = 0, string? search = null, bool trackChanges = false)
    {
        if (page < 0 || limit < 0)
        {
            throw new ArgumentOutOfRangeException("Page and limit must be non-negative.");
        }

        var query = GetQuery(trackChanges);

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(u => u.UserName.Contains(search) || u.Email.Contains(search));
        }

        if (limit > 0)
        {
            query = query.Skip(page * limit).Take(limit);
        }

        return await query.ToListAsync();
    }

    public Task<AppUser?> GetByIdAsync(string id, bool trackChanges = false)
    {
        return GetQuery(trackChanges)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public Task UpdateAsync(AppUser entity)
    {
        Context.Update(entity);
        return Task.CompletedTask;
    }

    private IQueryable<AppUser> GetQuery(bool trackChanges = false)
    {
        var query = Context.Users
            .AsQueryable();

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        query = query
            .Include(u => u.MatchesAsPlayer1)
                .ThenInclude(m => m.TicTacToeMatchHistory)
            .Include(u => u.MatchesAsPlayer2)
                .ThenInclude(m => m.TicTacToeMatchHistory);

        return query;
    }

    public Task<bool> ExistsAsync(string id)
    {
        return Context.Users.AnyAsync(u => u.Id == id);
    }

    public Task<bool> ValidAsync(string username, string hashedPassword)
    {
        return Context.Users.AnyAsync(u => u.UserName == username && u.HashedPassword == hashedPassword);
    }
}