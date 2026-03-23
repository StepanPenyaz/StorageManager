using Storage.Services.Models;

namespace Storage.Services.Interfaces;

public interface IStorageInitializationService
{
    Task<StorageInitializationResult> InitializeAsync(StorageInitializationRequest request);

    Task<BsxFileMetadata> GetBsxFileMetadataAsync(string filePath);

    Task<BsxFileProcessingResult> ProcessBsxFileAsync(BsxFileProcessingRequest request);
}
