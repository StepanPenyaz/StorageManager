using Storage.Domain.Enums;

namespace Storage.Services.Models;

public class StorageInitializationRequest
{
    public required int StartIndex { get; init; }

    public required IReadOnlyCollection<CabinetConfiguration> Cabinets { get; init; }

    public string? BsxFileName { get; init; }

    public string? BsxFileContentBase64 { get; init; }
}

public class CabinetConfiguration
{
    public required int CabinetIndex { get; init; }

    public required int GroupColumnsCount { get; init; }

    public required IReadOnlyCollection<ShelfConfiguration> Shelves { get; init; }
}

public class ShelfConfiguration
{
    public required int ShelfIndex { get; init; }

    public required IReadOnlyCollection<ContainerType> RowTypes { get; init; }
}
