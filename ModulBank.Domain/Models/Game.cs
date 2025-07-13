using ModulBank.Domain.Enums;

namespace ModulBank.Domain.Models;

public class Game
{
    public Guid Id { get; set; }
    public int Size { get; set; }
    public GameStatus Status { get; set; }
    public Player? Player1 { get; set; }
    public Player? Player2 { get; set; }
    public Player? CurrentPlayer { get; set; }
    public List<Cell> Cells { get; set; } = new();
    public int MovesCount { get; set; }
    public string ETag { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastMoveAt { get; set; }
} 