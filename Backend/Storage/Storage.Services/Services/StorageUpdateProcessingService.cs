using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Storage.Data.Contexts;
using Storage.Domain.Models;
using Storage.Services.Interfaces;
using Storage.Services.Models;

namespace Storage.Services.Services;

public class StorageUpdateProcessingService(
    StorageContext context,
    IStorageUpdateFileParser storageUpdateFileParser) : IStorageUpdateProcessingService
{
    private static readonly Regex ContainerRemarksRegex = new("^#(?<number>[0-9]+)$", RegexOptions.Compiled);

    public async Task<OrderProcessingResult> ProcessStorageUpdatesAsync(string inputDirectory, string finishedDirectory)
    {
        if (!Directory.Exists(inputDirectory))
            throw new DirectoryNotFoundException($"Storage update input directory not found: {inputDirectory}");

        var files = Directory.GetFiles(inputDirectory);
        var warnings = new List<string>();
        var itemsProcessed = 0;
        var errorCount = 0;
        var filesProcessed = 0;

        foreach (var filePath in files)
        {
            try
            {
                var (fileItems, fileWarnings) = await ProcessFileAsync(filePath, finishedDirectory);
                itemsProcessed += fileItems;
                warnings.AddRange(fileWarnings);
                filesProcessed++;
            }
            catch (Exception ex)
            {
                warnings.Add($"File '{Path.GetFileName(filePath)}' could not be processed: {ex.Message}");
                errorCount++;
            }
        }

        return new OrderProcessingResult
        {
            FilesProcessed = filesProcessed,
            ItemsProcessed = itemsProcessed,
            WarningCount = warnings.Count,
            ErrorCount = errorCount,
            Warnings = warnings
        };
    }

    private async Task<(int itemsProcessed, IReadOnlyCollection<string> warnings)> ProcessFileAsync(string filePath, string finishedDirectory)
    {
        var items = storageUpdateFileParser.Parse(filePath);
        var containerCache = new Dictionary<int, Container?>();
        var warnings = new List<string>();
        var itemsProcessed = 0;

        foreach (var item in items)
        {
            if (!TryParseContainerNumber(item.Remarks, out var containerNumber))
            {
                warnings.Add($"Item with ItemID '{item.ItemId}' has an invalid Remarks value '{item.Remarks}' and was skipped.");
                continue;
            }

            if (!containerCache.TryGetValue(containerNumber, out var container))
            {
                container = await context.Containers
                    .Include(c => c.Sections)
                        .ThenInclude(s => s.LotSections)
                    .FirstOrDefaultAsync(c => c.Number == containerNumber);

                containerCache[containerNumber] = container;
            }

            if (container is null)
            {
                warnings.Add($"Container #{containerNumber} was not found. ItemID '{item.ItemId}' was skipped.");
                continue;
            }

            var lotSection = await FindLotSectionByItemIdAsync(container, item.ItemId);

            if (lotSection is null)
            {
                warnings.Add($"No section with ItemID '{item.ItemId}' was found in container #{containerNumber}. Item was skipped.");
                continue;
            }

            lotSection.AddQuantity(item.Qty);
            itemsProcessed++;
        }

        await context.SaveChangesAsync();

        MoveFileToFinished(filePath, finishedDirectory);

        return (itemsProcessed, warnings);
    }

    private async Task<LotSection?> FindLotSectionByItemIdAsync(Container container, string itemId)
    {
        var allLotIds = container.Sections
            .SelectMany(s => s.LotSections)
            .Select(ls => ls.LotId)
            .Distinct()
            .ToList();

        if (allLotIds.Count == 0)
            return null;

        var matchingLotId = await context.Lots
            .Where(l => allLotIds.Contains(l.Id) && l.ItemId == itemId)
            .Select(l => (int?)l.Id)
            .FirstOrDefaultAsync();

        if (matchingLotId is null)
            return null;

        return container.Sections
            .SelectMany(s => s.LotSections)
            .FirstOrDefault(ls => ls.LotId == matchingLotId.Value);
    }

    private static void MoveFileToFinished(string filePath, string finishedDirectory)
    {
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var extension = Path.GetExtension(filePath);
        var doneFileName = $"{fileName}_done{extension}";
        var destinationPath = Path.Combine(finishedDirectory, doneFileName);

        File.Move(filePath, destinationPath);
    }

    private static bool TryParseContainerNumber(string remarks, out int containerNumber)
    {
        containerNumber = 0;
        var match = ContainerRemarksRegex.Match(remarks);
        return match.Success && int.TryParse(match.Groups["number"].Value, out containerNumber);
    }
}
