using Microsoft.EntityFrameworkCore;
using Storage.Data.Contexts;
using Storage.Data.Interfaces;
using Storage.Domain.Models;

namespace Storage.Data.Repositories;

public class StorageRepository(StorageContext context) : IStorageRepository
{
    public async Task<IReadOnlyCollection<Container>> GetContainersWithEmptySectionsAsync()
    {
        var containers = await context.Containers
            .Include(container => container.Sections)
            .AsNoTracking()
            .ToListAsync();

        foreach (var container in containers)
        {
            container.InitializeSections();
        }

        return containers;
    }
}
