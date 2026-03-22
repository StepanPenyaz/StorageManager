using Storage.Domain.Models;

namespace Storage.Data.Interfaces;

public interface IStorageRepository
{
    Task<IReadOnlyCollection<Container>> GetContainersWithEmptySectionsAsync();

    Task<Container?> GetContainerByNumberAsync(int containerNumber);

    Task<IReadOnlyCollection<int>> GetCabinetNumbersAsync();

    Task<IReadOnlyCollection<Container>> GetContainersByCabinetAsync(int cabinetNumber);

    void RemoveLotSection(LotSection lotSection);

    Task SaveChangesAsync();
}
