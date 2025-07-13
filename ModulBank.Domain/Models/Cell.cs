using ModulBank.Domain.Enums;

namespace ModulBank.Domain.Models;

public class Cell
{
    public int Row { get; set; }
    public int Column { get; set; }
    public CellValue Value { get; set; }
} 