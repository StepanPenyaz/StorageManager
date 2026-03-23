using System.Xml.Linq;
using Storage.Services.Interfaces;
using Storage.Services.Models;

namespace Storage.Services.Services;

public class StorageUpdateFileParser : IStorageUpdateFileParser
{
    public IReadOnlyCollection<StorageUpdateItem> Parse(string filePath) =>
        XDocument.Load(filePath)
            .Descendants("Inventory")
            .Elements("Item")
            .Where(item => item.Element("ItemID") != null && item.Element("Qty") != null)
            .Select(item => new StorageUpdateItem
            {
                ItemId = (string)item.Element("ItemID")!,
                Qty = (int)item.Element("Qty")!,
                Remarks = (string?)item.Element("Remarks") ?? string.Empty
            })
            .ToList();
}
