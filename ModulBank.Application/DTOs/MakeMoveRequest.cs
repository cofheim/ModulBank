namespace ModulBank.Application.DTOs;

public class MakeMoveRequest
{
    public int Row { get; set; }
    public int Column { get; set; }
    public string PlayerSessionId { get; set; } = string.Empty;
    public string? ETag { get; set; }
} 