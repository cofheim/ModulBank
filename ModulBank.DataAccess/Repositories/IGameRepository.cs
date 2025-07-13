using ModulBank.DataAccess.Entities;

namespace ModulBank.DataAccess.Repositories;

public interface IGameRepository
{
    Task<GameEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<GameEntity> CreateAsync(GameEntity game, CancellationToken cancellationToken);
    Task<GameEntity> UpdateAsync(GameEntity game, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
} 