using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Storage.Data.Contexts;

public class StorageContextFactory : IDesignTimeDbContextFactory<StorageContext>
{
    public StorageContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var connectionString = configuration.GetConnectionString("StorageDatabase")
            ?? throw new InvalidOperationException("Connection string 'StorageDatabase' not found.");

        var optionsBuilder = new DbContextOptionsBuilder<StorageContext>()
            .UseSqlServer(connectionString);

        return new StorageContext(optionsBuilder.Options);
    }
}
