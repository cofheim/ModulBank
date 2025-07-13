using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ModulBank.Application.DTOs;
using ModulBank.Application.Interfaces;
using ModulBank.DataAccess.Entities;
using ModulBank.DataAccess.Repositories;
using ModulBank.Domain.Enums;

namespace ModulBank.Application.Services;

public class GameService : IGameService
{
    private readonly IGameRepository _gameRepository;
    private readonly Random _random;
    private readonly int _defaultSize;
    private readonly int _winCondition;

    public GameService(IGameRepository gameRepository, IConfiguration configuration)
    {
        _gameRepository = gameRepository;
        _random = new Random();
        _defaultSize = configuration.GetValue<int>("Game:DefaultSize", 3);
        _winCondition = configuration.GetValue<int>("Game:WinCondition", 3);
    }

    public async Task<GameResponse> CreateGameAsync(CreateGameRequest request, CancellationToken cancellationToken)
    {
        var size = request.Size ?? _defaultSize;
        if (size < 3)
            throw new ArgumentException("Size must be at least 3");

        var game = new GameEntity
        {
            Id = Guid.NewGuid(),
            Size = size,
            Status = GameStatus.Created,
            CreatedAt = DateTime.UtcNow,
            ETag = Guid.NewGuid().ToString()
        };

        var player = new PlayerEntity
        {
            Id = Guid.NewGuid(),
            Symbol = CellValue.X,
            SessionId = request.PlayerSessionId
        };

        game.Player1 = player;
        game.Player1Id = player.Id;
        game.CurrentPlayer = player;
        game.CurrentPlayerId = player.Id;

        // Инициализация пустого поля
        for (var row = 0; row < size; row++)
        {
            for (var col = 0; col < size; col++)
            {
                game.Cells.Add(new CellEntity
                {
                    Id = Guid.NewGuid(),
                    Row = row,
                    Column = col,
                    Value = CellValue.Empty,
                    GameId = game.Id
                });
            }
        }

        game = await _gameRepository.CreateAsync(game, cancellationToken);
        return MapToResponse(game, request.PlayerSessionId);
    }

    public async Task<GameResponse> GetGameAsync(Guid id, string? playerSessionId, CancellationToken cancellationToken)
    {
        var game = await GetGameWithRelatedDataAsync(id, cancellationToken);
        return MapToResponse(game, playerSessionId);
    }

    public async Task<GameResponse> MakeMoveAsync(Guid id, MakeMoveRequest request, CancellationToken cancellationToken)
    {
        var game = await GetGameWithRelatedDataAsync(id, cancellationToken);

        // Проверка ETag для идемпотентности
        if (!string.IsNullOrEmpty(request.ETag) && game.ETag == request.ETag)
        {
            return MapToResponse(game, request.PlayerSessionId);
        }

        ValidateMove(game, request);

        var currentPlayer = GetOrCreatePlayer(game, request.PlayerSessionId);
        var cell = game.Cells.First(c => c.Row == request.Row && c.Column == request.Column);

        // Проверка на случайную замену символа (10% шанс на каждый третий ход)
        var symbol = currentPlayer.Symbol;
        if (game.MovesCount > 0 && game.MovesCount % 3 == 0 && _random.NextDouble() < 0.1)
        {
            symbol = symbol == CellValue.X ? CellValue.O : CellValue.X;
        }

        cell.Value = symbol;
        game.MovesCount++;
        game.LastMoveAt = DateTime.UtcNow;
        game.ETag = Guid.NewGuid().ToString();

        if (CheckWin(game, request.Row, request.Column))
        {
            game.Status = GameStatus.Won;
        }
        else if (game.Cells.All(c => c.Value != CellValue.Empty))
        {
            game.Status = GameStatus.Draw;
        }
        else
        {
            // Смена текущего игрока
            game.CurrentPlayerId = game.CurrentPlayerId == game.Player1Id ? game.Player2Id : game.Player1Id;
            game.Status = GameStatus.InProgress;
        }

        game = await _gameRepository.UpdateAsync(game, cancellationToken);
        return MapToResponse(game, request.PlayerSessionId);
    }

    private async Task<GameEntity> GetGameWithRelatedDataAsync(Guid id, CancellationToken cancellationToken)
    {
        var game = await _gameRepository.GetByIdAsync(id, cancellationToken);
        if (game == null)
            throw new KeyNotFoundException("Game not found");

        return game;
    }

    private void ValidateMove(GameEntity game, MakeMoveRequest request)
    {
        if (game.Status is GameStatus.Won or GameStatus.Draw)
            throw new InvalidOperationException("Game is already finished");

        if (game.CurrentPlayer?.SessionId != request.PlayerSessionId)
            throw new InvalidOperationException("It's not your turn");

        if (request.Row < 0 || request.Row >= game.Size || request.Column < 0 || request.Column >= game.Size)
            throw new ArgumentException("Invalid cell position");

        var cell = game.Cells.First(c => c.Row == request.Row && c.Column == request.Column);
        if (cell.Value != CellValue.Empty)
            throw new InvalidOperationException("Cell is already occupied");
    }

    private PlayerEntity GetOrCreatePlayer(GameEntity game, string sessionId)
    {
        if (game.Player1?.SessionId == sessionId)
            return game.Player1;

        if (game.Player2?.SessionId == sessionId)
            return game.Player2;

        if (game.Player2 != null)
            throw new InvalidOperationException("Game is full");

        var player = new PlayerEntity
        {
            Id = Guid.NewGuid(),
            Symbol = CellValue.O,
            SessionId = sessionId
        };

        game.Player2 = player;
        game.Player2Id = player.Id;
        return player;
    }

    private bool CheckWin(GameEntity game, int lastRow, int lastCol)
    {
        var value = game.Cells.First(c => c.Row == lastRow && c.Column == lastCol).Value;
        
        // Проверка по горизонтали
        var count = 0;
        for (var col = 0; col < game.Size; col++)
        {
            if (game.Cells.First(c => c.Row == lastRow && c.Column == col).Value == value)
                count++;
            else
                count = 0;
            
            if (count >= _winCondition)
                return true;
        }

        // Проверка по вертикали
        count = 0;
        for (var row = 0; row < game.Size; row++)
        {
            if (game.Cells.First(c => c.Row == row && c.Column == lastCol).Value == value)
                count++;
            else
                count = 0;
            
            if (count >= _winCondition)
                return true;
        }

        // Проверка по диагонали (сверху-слева вниз-направо)
        count = 0;
        var startRow = lastRow - Math.Min(lastRow, lastCol);
        var startCol = lastCol - Math.Min(lastRow, lastCol);
        while (startRow < game.Size && startCol < game.Size)
        {
            if (game.Cells.First(c => c.Row == startRow && c.Column == startCol).Value == value)
                count++;
            else
                count = 0;
            
            if (count >= _winCondition)
                return true;

            startRow++;
            startCol++;
        }

        // Проверка по диагонали (сверху-справа вниз-налево)
        count = 0;
        startRow = lastRow - Math.Min(lastRow, game.Size - 1 - lastCol);
        startCol = lastCol + Math.Min(lastRow, game.Size - 1 - lastCol);
        while (startRow < game.Size && startCol >= 0)
        {
            if (game.Cells.First(c => c.Row == startRow && c.Column == startCol).Value == value)
                count++;
            else
                count = 0;
            
            if (count >= _winCondition)
                return true;

            startRow++;
            startCol--;
        }

        return false;
    }

    private static GameResponse MapToResponse(GameEntity game, string? playerSessionId)
    {
        var isPlayer = !string.IsNullOrEmpty(playerSessionId) &&
                      (game.Player1?.SessionId == playerSessionId || game.Player2?.SessionId == playerSessionId);

        return new GameResponse
        {
            Id = game.Id,
            Size = game.Size,
            Status = game.Status,
            CurrentPlayerSessionId = isPlayer ? game.CurrentPlayer?.SessionId : null,
            Cells = game.Cells.Select(c => new CellDto
            {
                Row = c.Row,
                Column = c.Column,
                Value = c.Value
            }).ToList(),
            ETag = game.ETag,
            IsYourTurn = game.CurrentPlayer?.SessionId == playerSessionId,
            YourSymbol = isPlayer
                ? (game.Player1?.SessionId == playerSessionId ? game.Player1?.Symbol : game.Player2?.Symbol)
                : null
        };
    }
} 