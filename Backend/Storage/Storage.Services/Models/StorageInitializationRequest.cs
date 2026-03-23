using Storage.Domain.Enums;

namespace Storage.Services.Models;

public class StorageInitializationRequest
{
    public required int StartIndex { get; init; }

    public required IReadOnlyCollection<CabinetConfiguration> Cabinets { get; init; }
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

public class BsxFileMetadata
{
    public required string FileName { get; init; }

    public required string FilePath { get; init; }

    public required long FileSizeBytes { get; init; }
}

public class BsxFileProcessingRequest
{
    public required string FilePath { get; init; }

    public int? BatchSize { get; init; }
}

public class BsxFileProcessingResult
{
    public required string Status { get; init; }

    public required int ProcessedItemCount { get; init; }

    public required int CreatedLotCount { get; init; }

    public required int UpdatedLotCount { get; init; }

    public required int WarningCount { get; init; }

    public required int ErrorCount { get; init; }

    public required long ElapsedMilliseconds { get; init; }

    public required string SummaryMessage { get; init; }

    public required IReadOnlyCollection<string> Warnings { get; init; }
}
