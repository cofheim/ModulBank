namespace ModulBank.Application.DTOs;

public class CreateGameRequest
{
    public int? Size { get; set; }
    public string PlayerSessionId { get; set; } = string.Empty;
} 