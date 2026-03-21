using Storage.Domain.Models;

namespace Storage.Data.Interfaces;

public interface IStorageRepository
{
    Task<IReadOnlyCollection<Container>> GetContainersWithEmptySectionsAsync();
}
