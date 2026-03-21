namespace Storage.Domain.Models;

public class Location(int cabinet, int shelf, int groupRow, int groupColumn, int positionRow, int positionColumn)
{
    public int Cabinet { get; } = cabinet >= 0 
        ? cabinet 
        : throw new ArgumentOutOfRangeException(nameof(cabinet));

    public int Shelf { get; } = shelf >= 0 
        ? shelf 
        : throw new ArgumentOutOfRangeException(nameof(shelf));

    public int GroupRow { get; } = groupRow >= 0 
        ? groupRow 
        : throw new ArgumentOutOfRangeException(nameof(groupRow));

    public int GroupColumn { get; } = groupColumn >= 0 
        ? groupColumn 
        : throw new ArgumentOutOfRangeException(nameof(groupColumn));

    public int PositionRow { get; } = positionRow >= 0 
        ? positionRow 
        : throw new ArgumentOutOfRangeException(nameof(positionRow));

    public int PositionColumn { get; } = positionColumn >= 0 
        ? positionColumn 
        : throw new ArgumentOutOfRangeException(nameof(positionColumn));
}
