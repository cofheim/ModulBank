using Microsoft.EntityFrameworkCore;
using Xunit;
using ModulBank.DataAccess.Repositories;
using ModulBank.Domain.Models;
using ModulBank.Domain.Enums;
using ModulBank.DataAccess;

namespace ModulBank.Tests.Integration;

public class GameRepositoryTests : IDisposable
{
    private readonly GameDbContext _context;
    private readonly IGameRepository _repository;

    public GameRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<GameDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new GameDbContext(options);
        _repository = new GameRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task CreateGame_SavesGameToDatabase()
    {
        // Arrange
        var game = new Game
        {
            Id = Guid.NewGuid(),
            Size = 3,
            Status = GameStatus.InProgress,
            CurrentPlayer = CellValue.X,
            Board = new List<Cell>()
        };

        // Act
        var result = await _repository.CreateGameAsync(game);

        // Assert
        Assert.NotNull(result);
        var savedGame = await _context.Games.FindAsync(game.Id);
        Assert.NotNull(savedGame);
        Assert.Equal(game.Size, savedGame.Size);
        Assert.Equal(game.Status, savedGame.Status);
    }

    [Fact]
    public async Task GetGameById_ReturnsGame()
    {
        // Arrange
        var game = new Game
        {
            Id = Guid.NewGuid(),
            Size = 3,
            Status = GameStatus.InProgress,
            CurrentPlayer = CellValue.X,
            Board = new List<Cell>()
        };
        await _repository.CreateGameAsync(game);

        // Act
        var result = await _repository.GetGameByIdAsync(game.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(game.Id, result.Id);
        Assert.Equal(game.Size, result.Size);
    }

    [Fact]
    public async Task UpdateGame_UpdatesGameInDatabase()
    {
        // Arrange
        var game = new Game
        {
            Id = Guid.NewGuid(),
            Size = 3,
            Status = GameStatus.InProgress,
            CurrentPlayer = CellValue.X,
            Board = new List<Cell>()
        };
        await _repository.CreateGameAsync(game);

        // Изменяем состояние игры
        game.Status = GameStatus.Won;
        game.Winner = CellValue.X;

        // Act
        var result = await _repository.UpdateGameAsync(game);

        // Assert
        Assert.NotNull(result);
        var updatedGame = await _context.Games.FindAsync(game.Id);
        Assert.NotNull(updatedGame);
        Assert.Equal(GameStatus.Won, updatedGame.Status);
        Assert.Equal(CellValue.X, updatedGame.Winner);
    }

    [Fact]
    public async Task UpdateGame_WithConcurrency_ThrowsException()
    {
        // Arrange
        var game = new Game
        {
            Id = Guid.NewGuid(),
            Size = 3,
            Status = GameStatus.InProgress,
            CurrentPlayer = CellValue.X,
            Board = new List<Cell>(),
            Version = new byte[] { 1 }
        };
        await _repository.CreateGameAsync(game);

        // Симулируем параллельное обновление
        var parallelGame = await _repository.GetGameByIdAsync(game.Id);
        await _repository.UpdateGameAsync(game); // Первое обновление

        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() =>
            _repository.UpdateGameAsync(parallelGame)); // Второе обновление должно вызвать исключение
    }
} 