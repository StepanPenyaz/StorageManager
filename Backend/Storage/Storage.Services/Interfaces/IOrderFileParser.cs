using Storage.Services.Models;

namespace Storage.Services.Interfaces;

public interface IOrderFileParser
{
    IReadOnlyCollection<OrderItem> Parse(string filePath);
}
