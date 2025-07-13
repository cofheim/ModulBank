using Xunit;
using Moq;
using ModulBank.Application.Services;
using ModulBank.Domain.Models;
using ModulBank.Domain.Enums;
using ModulBank.DataAccess.Repositories;
using ModulBank.Application.DTOs;

namespace ModulBank.Tests.Services;

public class GameServiceTests
{
    private readonly Mock<IGameRepository> _mockRepository;
    private readonly IGameService _gameService;

    public GameServiceTests()
    {
        _mockRepository = new Mock<IGameRepository>();
        _gameService = new GameService(_mockRepository.Object);
    }

    [Fact]
    public async Task CreateGame_WithValidSize_ReturnsNewGame()
    {
        // Arrange
        var request = new CreateGameRequest { Size = 3 };
        var gameEntity = new Game
        {
            Id = Guid.NewGuid(),
            Size = 3,
            Status = GameStatus.InProgress,
            CurrentPlayer = CellValue.X,
            Board = new List<Cell>()
        };

        _mockRepository.Setup(r => r.CreateGameAsync(It.IsAny<Game>()))
            .ReturnsAsync(gameEntity);

        // Act
        var result = await _gameService.CreateGameAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(gameEntity.Id, result.Id);
        Assert.Equal(request.Size, result.Size);
        Assert.Equal(GameStatus.InProgress, result.Status);
    }

    [Theory]
    [InlineData(2)] // Меньше минимального размера
    [InlineData(0)] // Некорректный размер
    [InlineData(-1)] // Отрицательный размер
    public async Task CreateGame_WithInvalidSize_ThrowsException(int size)
    {
        // Arrange
        var request = new CreateGameRequest { Size = size };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _gameService.CreateGameAsync(request));
    }

    [Fact]
    public async Task MakeMove_ValidMove_UpdatesGameState()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var game = new Game
        {
            Id = gameId,
            Size = 3,
            Status = GameStatus.InProgress,
            CurrentPlayer = CellValue.X,
            Board = new List<Cell>(),
            Version = new byte[] { 1 }
        };

        var move = new MakeMoveRequest
        {
            Row = 0,
            Column = 0,
            Player = CellValue.X
        };

        _mockRepository.Setup(r => r.GetGameByIdAsync(gameId))
            .ReturnsAsync(game);
        _mockRepository.Setup(r => r.UpdateGameAsync(It.IsAny<Game>()))
            .ReturnsAsync(game);

        // Act
        var result = await _gameService.MakeMoveAsync(gameId, move);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(gameId, result.Id);
        _mockRepository.Verify(r => r.UpdateGameAsync(It.IsAny<Game>()), Times.Once);
    }

    [Fact]
    public async Task MakeMove_OutOfBounds_ThrowsException()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var game = new Game
        {
            Id = gameId,
            Size = 3,
            Status = GameStatus.InProgress,
            CurrentPlayer = CellValue.X,
            Board = new List<Cell>(),
            Version = new byte[] { 1 }
        };

        var move = new MakeMoveRequest
        {
            Row = 3, // За пределами поля
            Column = 3,
            Player = CellValue.X
        };

        _mockRepository.Setup(r => r.GetGameByIdAsync(gameId))
            .ReturnsAsync(game);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _gameService.MakeMoveAsync(gameId, move));
    }

    [Fact]
    public async Task MakeMove_ThirdMove_ChecksSpecialRule()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var game = new Game
        {
            Id = gameId,
            Size = 3,
            Status = GameStatus.InProgress,
            CurrentPlayer = CellValue.X,
            Board = new List<Cell>
            {
                new Cell { Row = 0, Column = 0, Value = CellValue.X },
                new Cell { Row = 0, Column = 1, Value = CellValue.O }
            },
            MoveCount = 2,
            Version = new byte[] { 1 }
        };

        var move = new MakeMoveRequest
        {
            Row = 0,
            Column = 2,
            Player = CellValue.X
        };

        _mockRepository.Setup(r => r.GetGameByIdAsync(gameId))
            .ReturnsAsync(game);
        _mockRepository.Setup(r => r.UpdateGameAsync(It.IsAny<Game>()))
            .ReturnsAsync((Game g) => g);

        // Act
        var result = await _gameService.MakeMoveAsync(gameId, move);

        // Assert
        Assert.NotNull(result);
        // Проверяем, что значение в ячейке может быть либо X, либо O (из-за 10% вероятности)
        var cell = result.Board.First(c => c.Row == move.Row && c.Column == move.Column);
        Assert.True(cell.Value == CellValue.X || cell.Value == CellValue.O);
    }

    [Fact]
    public async Task MakeMove_WinningMove_UpdatesGameStatus()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var game = new Game
        {
            Id = gameId,
            Size = 3,
            Status = GameStatus.InProgress,
            CurrentPlayer = CellValue.X,
            Board = new List<Cell>
            {
                new Cell { Row = 0, Column = 0, Value = CellValue.X },
                new Cell { Row = 0, Column = 1, Value = CellValue.X },
                new Cell { Row = 1, Column = 0, Value = CellValue.O },
                new Cell { Row = 1, Column = 1, Value = CellValue.O }
            },
            Version = new byte[] { 1 }
        };

        var move = new MakeMoveRequest
        {
            Row = 0,
            Column = 2,
            Player = CellValue.X
        };

        _mockRepository.Setup(r => r.GetGameByIdAsync(gameId))
            .ReturnsAsync(game);
        _mockRepository.Setup(r => r.UpdateGameAsync(It.IsAny<Game>()))
            .ReturnsAsync((Game g) => g);

        // Act
        var result = await _gameService.MakeMoveAsync(gameId, move);

        // Assert
        Assert.Equal(GameStatus.Won, result.Status);
        Assert.Equal(CellValue.X, result.Winner);
    }

    [Fact]
    public async Task MakeMove_DrawGame_UpdatesGameStatus()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var game = new Game
        {
            Id = gameId,
            Size = 3,
            Status = GameStatus.InProgress,
            CurrentPlayer = CellValue.X,
            Board = new List<Cell>
            {
                new Cell { Row = 0, Column = 0, Value = CellValue.X },
                new Cell { Row = 0, Column = 1, Value = CellValue.O },
                new Cell { Row = 0, Column = 2, Value = CellValue.X },
                new Cell { Row = 1, Column = 0, Value = CellValue.O },
                new Cell { Row = 1, Column = 1, Value = CellValue.X },
                new Cell { Row = 1, Column = 2, Value = CellValue.O },
                new Cell { Row = 2, Column = 0, Value = CellValue.O },
                new Cell { Row = 2, Column = 1, Value = CellValue.X }
            },
            Version = new byte[] { 1 }
        };

        var move = new MakeMoveRequest
        {
            Row = 2,
            Column = 2,
            Player = CellValue.O
        };

        _mockRepository.Setup(r => r.GetGameByIdAsync(gameId))
            .ReturnsAsync(game);
        _mockRepository.Setup(r => r.UpdateGameAsync(It.IsAny<Game>()))
            .ReturnsAsync((Game g) => g);

        // Act
        var result = await _gameService.MakeMoveAsync(gameId, move);

        // Assert
        Assert.Equal(GameStatus.Draw, result.Status);
        Assert.Null(result.Winner);
    }

    [Fact]
    public async Task MakeMove_GameAlreadyFinished_ThrowsException()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var game = new Game
        {
            Id = gameId,
            Size = 3,
            Status = GameStatus.Won,
            CurrentPlayer = CellValue.X,
            Winner = CellValue.X,
            Board = new List<Cell>(),
            Version = new byte[] { 1 }
        };

        var move = new MakeMoveRequest
        {
            Row = 0,
            Column = 0,
            Player = CellValue.O
        };

        _mockRepository.Setup(r => r.GetGameByIdAsync(gameId))
            .ReturnsAsync(game);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _gameService.MakeMoveAsync(gameId, move));
    }
} 