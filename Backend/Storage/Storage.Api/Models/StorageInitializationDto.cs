namespace Storage.Api.Models;

public class StorageInitializationDto
{
    public required int StartIndex { get; init; }

    public required IReadOnlyCollection<CabinetInitializationDto> Cabinets { get; init; }

    public string? BsxFileName { get; init; }

    public string? BsxFileContentBase64 { get; init; }
}

public class CabinetInitializationDto
{
    public required int CabinetIndex { get; init; }

    public required int GroupColumnsCount { get; init; }

    public required IReadOnlyCollection<ShelfInitializationDto> Shelves { get; init; }
}

public class ShelfInitializationDto
{
    public required int ShelfIndex { get; init; }

    public required IReadOnlyCollection<string> RowTypes { get; init; }
}
