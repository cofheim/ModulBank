using Microsoft.EntityFrameworkCore;
using ModulBank.DataAccess.Entities;

namespace ModulBank.DataAccess.Repositories;

public class GameRepository : IGameRepository
{
    private readonly GameDbContext _dbContext;

    public GameRepository(GameDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GameEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Games
            .Include(g => g.Player1)
            .Include(g => g.Player2)
            .Include(g => g.CurrentPlayer)
            .Include(g => g.Cells)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    public async Task<GameEntity> CreateAsync(GameEntity game, CancellationToken cancellationToken)
    {
        var entry = await _dbContext.Games.AddAsync(game, cancellationToken);
        await SaveChangesAsync(cancellationToken);
        return entry.Entity;
    }

    public async Task<GameEntity> UpdateAsync(GameEntity game, CancellationToken cancellationToken)
    {
        var entry = _dbContext.Games.Update(game);
        await SaveChangesAsync(cancellationToken);
        return entry.Entity;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
} 