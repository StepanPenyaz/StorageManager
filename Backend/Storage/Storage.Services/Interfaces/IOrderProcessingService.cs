using Storage.Services.Models;

namespace Storage.Services.Interfaces;

public interface IOrderProcessingService
{
    Task<OrderProcessingResult> ProcessOrdersAsync(string inputDirectory, string finishedDirectory);
}
