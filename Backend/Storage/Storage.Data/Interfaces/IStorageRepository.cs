using Storage.Domain.Models;

namespace Storage.Data.Interfaces;

public interface IStorageRepository
{
    Task<IReadOnlyCollection<Container>> GetContainersWithEmptySectionsAsync();

    Task<Container?> GetContainerByNumberAsync(int containerNumber);

    void RemoveLotSection(LotSection lotSection);

    Task SaveChangesAsync();
}
