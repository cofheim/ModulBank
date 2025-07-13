using Microsoft.AspNetCore.Mvc;
using ModulBank.Application.DTOs;
using ModulBank.Application.Interfaces;

namespace ModulBank.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly IGameService _gameService;

    public GameController(IGameService gameService)
    {
        _gameService = gameService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(GameResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateGameRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _gameService.CreateGameAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid request",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GameResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id, [FromQuery] string? playerSessionId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _gameService.GetGameAsync(id, playerSessionId, cancellationToken);
            return Ok(response);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Game not found",
                Detail = $"Game with ID {id} was not found",
                Status = StatusCodes.Status404NotFound
            });
        }
    }

    [HttpPost("{id:guid}/moves")]
    [ProducesResponseType(typeof(GameResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MakeMove(Guid id, [FromBody] MakeMoveRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _gameService.MakeMoveAsync(id, request, cancellationToken);
            return Ok(response);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Game not found",
                Detail = $"Game with ID {id} was not found",
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid move",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid request",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok();
    }
} 