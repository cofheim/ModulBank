using ModulBank.Application.DTOs;

namespace ModulBank.Application.Interfaces;

public interface IGameService
{
    Task<GameResponse> CreateGameAsync(CreateGameRequest request, CancellationToken cancellationToken);
    Task<GameResponse> GetGameAsync(Guid id, string? playerSessionId, CancellationToken cancellationToken);
    Task<GameResponse> MakeMoveAsync(Guid id, MakeMoveRequest request, CancellationToken cancellationToken);
} 