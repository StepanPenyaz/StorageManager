namespace Storage.Api.Models;

public class StorageInitializationDto
{
    public required int StartIndex { get; init; }

    public required IReadOnlyCollection<CabinetInitializationDto> Cabinets { get; init; }
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

public class BsxFilePathDto
{
    public required string FilePath { get; init; }
}

public class BsxFileMetadataDto
{
    public required string FileName { get; init; }

    public required string FilePath { get; init; }

    public required long FileSizeBytes { get; init; }
}

public class BsxFileProcessingDto
{
    public required string FilePath { get; init; }

    public int? BatchSize { get; init; }
}

public class BsxFileProcessingResultDto
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
