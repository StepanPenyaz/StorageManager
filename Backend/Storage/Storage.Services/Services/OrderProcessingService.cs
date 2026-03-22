using Microsoft.Extensions.Configuration;
using Storage.Data.Interfaces;
using Storage.Domain.Models;
using Storage.Services.Interfaces;

namespace Storage.Services.Services;

public class OrderProcessingService(
    IStorageRepository repository,
    IOrderFileParser orderFileParser,
    IConfiguration configuration) : IOrderProcessingService
{
    private string InputDirectory => configuration["OrderProcessing:InputDirectory"]
        ?? throw new InvalidOperationException("OrderProcessing:InputDirectory is not configured.");

    private string FinishedDirectory => configuration["OrderProcessing:FinishedDirectory"]
        ?? throw new InvalidOperationException("OrderProcessing:FinishedDirectory is not configured.");

    public async Task ProcessOrdersAsync()
    {
        if (!Directory.Exists(InputDirectory))
            throw new DirectoryNotFoundException($"Order input directory not found: {InputDirectory}");

        var files = Directory.GetFiles(InputDirectory);

        foreach (var filePath in files)
            await ProcessFileAsync(filePath);
    }

    private async Task ProcessFileAsync(string filePath)
    {
        var items = orderFileParser.Parse(filePath);
        var containerCache = new Dictionary<int, Container?>();

        foreach (var item in items)
        {
            if (!TryParseContainerNumber(item.Remarks, out var containerNumber))
                continue;

            if (!containerCache.TryGetValue(containerNumber, out var container))
            {
                container = await repository.GetContainerByNumberAsync(containerNumber);
                containerCache[containerNumber] = container;
            }

            if (container is null)
                continue;

            var section = container.Sections
                .FirstOrDefault(s => s.LotSections.Any(ls => ls.LotId == item.LotId));

            if (section is null)
                continue;

            var lotSection = section.LotSections.First(ls => ls.LotId == item.LotId);

            lotSection.SubtractQuantity(item.Qty);

            if (lotSection.Quantity == 0)
                repository.RemoveLotSection(lotSection);
        }

        await repository.SaveChangesAsync();

        MoveFileToFinished(filePath);
    }

    private void MoveFileToFinished(string filePath)
    {
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var extension = Path.GetExtension(filePath);
        var doneFileName = $"{fileName}_done{extension}";
        var destinationPath = Path.Combine(FinishedDirectory, doneFileName);

        File.Move(filePath, destinationPath);
    }

    private static bool TryParseContainerNumber(string remarks, out int containerNumber)
    {
        containerNumber = 0;
        return remarks.StartsWith('#') && int.TryParse(remarks[1..], out containerNumber);
    }
}
