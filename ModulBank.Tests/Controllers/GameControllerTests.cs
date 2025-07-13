using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using ModulBank.Controllers;
using ModulBank.Application.Services;
using ModulBank.Application.DTOs;
using ModulBank.Domain.Models;
using ModulBank.Domain.Enums;

namespace ModulBank.Tests.Controllers;

public class GameControllerTests
{
    private readonly Mock<IGameService> _mockGameService;
    private readonly GameController _controller;

    public GameControllerTests()
    {
        _mockGameService = new Mock<IGameService>();
        _controller = new GameController(_mockGameService.Object);
    }

    [Fact]
    public async Task CreateGame_ValidRequest_ReturnsOkResult()
    {
        // Arrange
        var request = new CreateGameRequest { Size = 3 };
        var gameResponse = new GameResponse
        {
            Id = Guid.NewGuid(),
            Size = 3,
            Status = GameStatus.InProgress,
            Board = new List<Cell>()
        };

        _mockGameService.Setup(s => s.CreateGameAsync(request))
            .ReturnsAsync(gameResponse);

        // Act
        var result = await _controller.CreateGame(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<GameResponse>(okResult.Value);
        Assert.Equal(gameResponse.Id, returnValue.Id);
    }

    [Fact]
    public async Task CreateGame_InvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateGameRequest { Size = 2 }; // Некорректный размер
        _mockGameService.Setup(s => s.CreateGameAsync(request))
            .ThrowsAsync(new ArgumentException("Invalid board size"));

        // Act
        var result = await _controller.CreateGame(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var problemDetails = Assert.IsType<ProblemDetails>(badRequestResult.Value);
        Assert.Equal(400, problemDetails.Status);
    }

    [Fact]
    public async Task GetGame_ExistingGame_ReturnsOkResult()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var gameResponse = new GameResponse
        {
            Id = gameId,
            Size = 3,
            Status = GameStatus.InProgress,
            Board = new List<Cell>()
        };

        _mockGameService.Setup(s => s.GetGameAsync(gameId))
            .ReturnsAsync(gameResponse);

        // Act
        var result = await _controller.GetGame(gameId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<GameResponse>(okResult.Value);
        Assert.Equal(gameId, returnValue.Id);
    }

    [Fact]
    public async Task GetGame_NonExistingGame_ReturnsNotFound()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        _mockGameService.Setup(s => s.GetGameAsync(gameId))
            .ReturnsAsync((GameResponse)null);

        // Act
        var result = await _controller.GetGame(gameId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task MakeMove_ValidMove_ReturnsOkResult()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var move = new MakeMoveRequest
        {
            Row = 0,
            Column = 0,
            Player = CellValue.X
        };

        var gameResponse = new GameResponse
        {
            Id = gameId,
            Size = 3,
            Status = GameStatus.InProgress,
            Board = new List<Cell>()
        };

        _mockGameService.Setup(s => s.MakeMoveAsync(gameId, move))
            .ReturnsAsync(gameResponse);

        // Act
        var result = await _controller.MakeMove(gameId, move);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<GameResponse>(okResult.Value);
        Assert.Equal(gameId, returnValue.Id);
    }

    [Fact]
    public async Task MakeMove_InvalidMove_ReturnsBadRequest()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var move = new MakeMoveRequest
        {
            Row = -1,
            Column = 0,
            Player = CellValue.X
        };

        _mockGameService.Setup(s => s.MakeMoveAsync(gameId, move))
            .ThrowsAsync(new ArgumentException("Invalid move"));

        // Act
        var result = await _controller.MakeMove(gameId, move);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var problemDetails = Assert.IsType<ProblemDetails>(badRequestResult.Value);
        Assert.Equal(400, problemDetails.Status);
    }

    [Fact]
    public async Task Health_ReturnsOk()
    {
        // Act
        var result = _controller.Health();

        // Assert
        Assert.IsType<OkResult>(result);
    }
} 