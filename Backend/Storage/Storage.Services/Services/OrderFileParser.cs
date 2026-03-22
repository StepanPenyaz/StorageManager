using System.Xml.Linq;
using Storage.Services.Interfaces;
using Storage.Services.Models;

namespace Storage.Services.Services;

public class OrderFileParser : IOrderFileParser
{
    public IReadOnlyCollection<OrderItem> Parse(string filePath) =>
        XDocument.Load(filePath)
            .Descendants("Inventory")
            .Elements("Item")
            .Where(item => item.Element("LotID") != null && item.Element("Qty") != null)
            .Select(item => new OrderItem
            {
                LotId = (int)item.Element("LotID")!,
                Qty = (int)item.Element("Qty")!,
                Remarks = (string?)item.Element("Remarks") ?? string.Empty
            })
            .ToList();
}
