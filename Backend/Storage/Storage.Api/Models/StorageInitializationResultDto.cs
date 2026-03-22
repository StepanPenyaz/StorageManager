using Storage.Domain.Enums;

namespace Storage.Api.Models;

public class StorageInitializationResultDto
{
    public required int ContainersCreated { get; init; }

    public required int SectionsCreated { get; init; }

    public required IReadOnlyDictionary<ContainerType, int> ContainersByType { get; init; }
}
