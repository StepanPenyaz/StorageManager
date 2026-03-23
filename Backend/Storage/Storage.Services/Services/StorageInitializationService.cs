using Microsoft.EntityFrameworkCore;
using Storage.Data.Contexts;
using Storage.Domain.Enums;
using Storage.Domain.Models;
using Storage.Services.Interfaces;
using Storage.Services.Models;

namespace Storage.Services.Services;

public class StorageInitializationService(StorageContext context) : IStorageInitializationService
{
    private static readonly IReadOnlyDictionary<ContainerType, (int rows, int columns)> LayoutMap = new Dictionary<ContainerType, (int rows, int columns)>
    {
        [ContainerType.PX12] = (3, 4),
        [ContainerType.PX6] = (3, 2),
        [ContainerType.PX4] = (2, 2),
        [ContainerType.PX2] = (1, 2)
    };

    public async Task<StorageInitializationResult> InitializeAsync(StorageInitializationRequest request)
    {
        ValidateRequest(request);

        if (await context.Containers.AnyAsync())
            throw new InvalidOperationException("Storage is already initialized.");

        var groups = new List<ContainerGroup>();
        var containersByType = new Dictionary<ContainerType, int>();
        var currentNumber = request.StartIndex;

        foreach (var cabinet in request.Cabinets.OrderBy(c => c.CabinetIndex))
        {
            foreach (var shelf in cabinet.Shelves.OrderBy(s => s.ShelfIndex))
            {
                var rowTypes = shelf.RowTypes.ToList();

                for (var groupRow = 1; groupRow <= rowTypes.Count; groupRow++)
                {
                    for (var groupColumn = 1; groupColumn <= cabinet.GroupColumnsCount; groupColumn++)
                    {
                        var type = rowTypes[groupRow - 1];
                        var group = new ContainerGroup(type, cabinet.CabinetIndex, shelf.ShelfIndex, groupRow, groupColumn);
                        var layout = LayoutMap[type];

                        for (var positionRow = 1; positionRow <= layout.rows; positionRow++)
                        {
                            for (var positionColumn = 1; positionColumn <= layout.columns; positionColumn++)
                            {
                                var container = new Container(
                                    type,
                                    currentNumber,
                                    new Location(
                                        cabinet.CabinetIndex,
                                        shelf.ShelfIndex,
                                        groupRow,
                                        groupColumn,
                                        positionRow,
                                        positionColumn));

                                container.InitializeSections();
                                group.AddContainer(container);
                                IncrementContainerCount(containersByType, type);
                                currentNumber++;
                            }
                        }

                        groups.Add(group);
                    }
                }
            }
        }

        await context.ContainerGroups.AddRangeAsync(groups);
        await context.SaveChangesAsync();

        var sectionsCount = groups
            .SelectMany(g => g.Containers)
            .Sum(container => container.Sections.Count);

        return new StorageInitializationResult
        {
            ContainersCreated = currentNumber - request.StartIndex,
            SectionsCreated = sectionsCount,
            ContainersByType = containersByType
        };
    }

    private static void ValidateRequest(StorageInitializationRequest request)
    {
        if (request.StartIndex <= 0)
            throw new ArgumentOutOfRangeException(nameof(request.StartIndex), "Start index must be a positive integer.");

        if (request.Cabinets.Count == 0)
            throw new InvalidOperationException("At least one cabinet configuration is required.");

        foreach (var cabinet in request.Cabinets)
        {
            if (cabinet.GroupColumnsCount < 1)
                throw new ArgumentOutOfRangeException(nameof(cabinet.GroupColumnsCount), $"Cabinet {cabinet.CabinetIndex} must have at least 1 container group column.");

            if (cabinet.Shelves.Count == 0)
                throw new InvalidOperationException($"Cabinet {cabinet.CabinetIndex} must contain at least one shelf.");

            foreach (var shelf in cabinet.Shelves)
            {
                if (shelf.RowTypes.Count == 0)
                    throw new InvalidOperationException($"Shelf {shelf.ShelfIndex} in cabinet {cabinet.CabinetIndex} must provide at least one container group row type.");

                foreach (var type in shelf.RowTypes)
                {
                    if (!LayoutMap.ContainsKey(type))
                        throw new InvalidOperationException("Unknown container type provided.");
                }
            }
        }
    }

    private static void IncrementContainerCount(IDictionary<ContainerType, int> containersByType, ContainerType type)
    {
        if (!containersByType.TryGetValue(type, out var count))
            containersByType[type] = 1;
        else
            containersByType[type] = count + 1;
    }
}
