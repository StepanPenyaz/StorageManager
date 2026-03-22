using Storage.Domain.Enums;

namespace Storage.Services.Models;

public class StorageInitializationResult
{
    public required int ContainersCreated { get; init; }

    public required int SectionsCreated { get; init; }

    public required IReadOnlyDictionary<ContainerType, int> ContainersByType { get; init; }
}
