using ModulBank.Domain.Enums;

namespace ModulBank.DataAccess.Entities;

public class GameEntity
{
    public Guid Id { get; set; }
    public int Size { get; set; }
    public GameStatus Status { get; set; }
    public int MovesCount { get; set; }
    public string ETag { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastMoveAt { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public Guid? Player1Id { get; set; }
    public PlayerEntity? Player1 { get; set; }
    
    public Guid? Player2Id { get; set; }
    public PlayerEntity? Player2 { get; set; }
    
    public Guid? CurrentPlayerId { get; set; }
    public PlayerEntity? CurrentPlayer { get; set; }
    
    public List<CellEntity> Cells { get; set; } = new();
} 