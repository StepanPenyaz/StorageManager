namespace Storage.Api.Models;

public class ProcessingResultDto
{
    public required int FilesProcessed { get; init; }

    public required int ItemsProcessed { get; init; }

    public required int WarningCount { get; init; }

    public required int ErrorCount { get; init; }

    public required IReadOnlyCollection<string> Warnings { get; init; }
}
