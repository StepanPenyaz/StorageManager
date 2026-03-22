namespace Storage.Api.Models;

public class ContainerDto
{
    public required int Number { get; init; }

    public required string Type { get; init; }

    public required int Shelf { get; init; }

    public required int GroupRow { get; init; }

    public required int GroupColumn { get; init; }

    public required int PositionRow { get; init; }

    public required int PositionColumn { get; init; }

    public required int TotalSections { get; init; }

    public required int EmptySections { get; init; }
}
