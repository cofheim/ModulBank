using ModulBank.Domain.Enums;
using ModulBank.Domain.Models;

namespace ModulBank.Application.DTOs;

public class GameResponse
{
    public Guid Id { get; set; }
    public int Size { get; set; }
    public GameStatus Status { get; set; }
    public string? CurrentPlayerSessionId { get; set; }
    public List<CellDto> Cells { get; set; } = new();
    public string ETag { get; set; } = string.Empty;
    public bool IsYourTurn { get; set; }
    public CellValue? YourSymbol { get; set; }
}

public class CellDto
{
    public int Row { get; set; }
    public int Column { get; set; }
    public CellValue Value { get; set; }
} 