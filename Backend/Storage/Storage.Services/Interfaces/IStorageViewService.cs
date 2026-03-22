using Storage.Services.Models;

namespace Storage.Services.Interfaces;

public interface IStorageViewService
{
    Task<IReadOnlyCollection<int>> GetCabinetNumbersAsync();

    Task<IReadOnlyCollection<ContainerAvailability>> GetCabinetContainersAsync(int cabinetNumber);
}
