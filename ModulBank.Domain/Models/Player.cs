using ModulBank.Domain.Enums;

namespace ModulBank.Domain.Models;

public class Player
{
    public Guid Id { get; set; }
    public CellValue Symbol { get; set; }
    public string SessionId { get; set; } = string.Empty;
} 