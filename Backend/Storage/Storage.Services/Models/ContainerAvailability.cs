using Storage.Domain.Enums;

namespace Storage.Services.Models;

public class ContainerAvailability
{
    public required int Number { get; init; }

    public required ContainerType Type { get; init; }

    public required int Cabinet { get; init; }

    public required int Shelf { get; init; }

    public required int GroupRow { get; init; }

    public required int GroupColumn { get; init; }

    public required int PositionRow { get; init; }

    public required int PositionColumn { get; init; }

    public required int TotalSections { get; init; }

    public required int EmptySections { get; init; }
}
