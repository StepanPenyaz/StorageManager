using Storage.Services.Models;

namespace Storage.Services.Interfaces;

public interface IStorageInitializationService
{
    Task<StorageInitializationResult> InitializeAsync(StorageInitializationRequest request);
}
