using Storage.Services.Models;

namespace Storage.Services.Interfaces;

public interface IStorageUpdateFileParser
{
    IReadOnlyCollection<StorageUpdateItem> Parse(string filePath);
}
