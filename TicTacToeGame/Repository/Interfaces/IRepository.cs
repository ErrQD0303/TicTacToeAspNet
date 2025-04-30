namespace TicTacToeGame.Repository.Interfaces;

public interface IRepository<TEntity> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(string id, bool trackChanges = false);
    Task<IEnumerable<TEntity>> GetAllAsync(int page = 0, int limit = 0, string? search = null, bool trackChanges = false);
    Task AddAsync(TEntity entity);
    Task UpdateAsync(TEntity entity);
    Task DeleteAsync(TEntity entity);
    Task<bool> ExistsAsync(string id);
}