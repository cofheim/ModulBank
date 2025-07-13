using ModulBank.Domain.Enums;

namespace ModulBank.DataAccess.Entities;

public class PlayerEntity
{
    public Guid Id { get; set; }
    public CellValue Symbol { get; set; }
    public string SessionId { get; set; } = string.Empty;
    
    public Guid GameId { get; set; }
    public GameEntity Game { get; set; } = null!;
} 