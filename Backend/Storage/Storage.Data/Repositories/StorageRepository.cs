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
            .Where(container => container.Sections.Any(section => section.LotSections.Count == 0))
            .Include(container => container.Sections)
            .AsNoTracking()
            .ToListAsync();

        foreach (var container in containers)
        {
            container.InitializeSections();
        }

        return containers;
    }

    public async Task<Container?> GetContainerByNumberAsync(int containerNumber) =>
        await context.Containers
            .Include(container => container.Sections)
                .ThenInclude(section => section.LotSections)
            .FirstOrDefaultAsync(container => container.Number == containerNumber);

    public async Task<IReadOnlyCollection<int>> GetCabinetNumbersAsync() =>
        await context.Containers
            .Where(c => c.Sections.Any(s => s.LotSections.Count == 0))
            .Select(c => c.Location.Cabinet)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

    public async Task<IReadOnlyCollection<Container>> GetContainersByCabinetAsync(int cabinetNumber) =>
        await context.Containers
            .Where(c => c.Location.Cabinet == cabinetNumber && c.Sections.Any(s => s.LotSections.Count == 0))
            .Include(c => c.Sections)
                .ThenInclude(s => s.LotSections)
            .AsNoTracking()
            .ToListAsync();

    public void RemoveLotSection(LotSection lotSection) =>
        context.Remove(lotSection);

    public Task SaveChangesAsync() => context.SaveChangesAsync();
}
