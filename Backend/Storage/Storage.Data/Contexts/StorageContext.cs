using Microsoft.EntityFrameworkCore;
using Storage.Domain.Models;

namespace Storage.Data.Contexts;

public class StorageContext(DbContextOptions<StorageContext> options) : DbContext(options)
{
    public DbSet<Container> Containers => Set<Container>();

    public DbSet<Section> Sections => Set<Section>();

    public DbSet<Lot> Lots => Set<Lot>();

    public DbSet<LotSection> LotSections => Set<LotSection>();

    public DbSet<ContainerGroup> ContainerGroups => Set<ContainerGroup>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StorageContext).Assembly);
    }
}
