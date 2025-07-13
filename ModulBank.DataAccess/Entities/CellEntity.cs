using ModulBank.Domain.Enums;

namespace ModulBank.DataAccess.Entities;

public class CellEntity
{
    public Guid Id { get; set; }
    public int Row { get; set; }
    public int Column { get; set; }
    public CellValue Value { get; set; }
    
    public Guid GameId { get; set; }
    public GameEntity Game { get; set; } = null!;
} 