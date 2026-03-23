using Storage.Services.Models;

namespace Storage.Services.Interfaces;

public interface IStorageUpdateProcessingService
{
    Task<OrderProcessingResult> ProcessStorageUpdatesAsync(string inputDirectory, string finishedDirectory);
}
