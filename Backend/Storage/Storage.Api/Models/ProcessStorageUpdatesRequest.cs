namespace Storage.Api.Models;

public class ProcessStorageUpdatesRequest
{
    public required string IncomingStorageUpdatesFolder { get; init; }

    public required string ProcessedStorageUpdatesFolder { get; init; }
}
