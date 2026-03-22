using Storage.Data.Interfaces;
using Storage.Services.Interfaces;
using Storage.Services.Models;

namespace Storage.Services.Services;

public class StorageViewService(IStorageRepository repository) : IStorageViewService
{
    public Task<IReadOnlyCollection<int>> GetCabinetNumbersAsync() =>
        repository.GetCabinetNumbersAsync();

    public async Task<IReadOnlyCollection<ContainerAvailability>> GetCabinetContainersAsync(int cabinetNumber)
    {
        var containers = await repository.GetContainersByCabinetAsync(cabinetNumber);

        return containers
            .Select(c => new ContainerAvailability
            {
                Number = c.Number,
                Type = c.Type,
                Cabinet = c.Location.Cabinet,
                Shelf = c.Location.Shelf,
                GroupRow = c.Location.GroupRow,
                GroupColumn = c.Location.GroupColumn,
                PositionRow = c.Location.PositionRow,
                PositionColumn = c.Location.PositionColumn,
                TotalSections = c.Sections.Count,
                EmptySections = c.Sections.Count(s => s.LotSections.Count == 0)
            })
            .ToList();
    }
}
