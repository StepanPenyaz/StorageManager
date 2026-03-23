using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using Storage.Data.Contexts;
using Storage.Domain.Enums;
using Storage.Domain.Models;
using Storage.Services.Interfaces;
using Storage.Services.Models;

namespace Storage.Services.Services;

public class StorageInitializationService(StorageContext context) : IStorageInitializationService
{
    private const int DefaultBatchSize = 1000;
    private static readonly Regex ContainerRemarksRegex = new("^#(?<number>[0-9]+)$", RegexOptions.Compiled);

    private static readonly IReadOnlyDictionary<ContainerType, (int rows, int columns)> LayoutMap = new Dictionary<ContainerType, (int rows, int columns)>
    {
        [ContainerType.PX12] = (3, 4),
        [ContainerType.PX6] = (3, 2),
        [ContainerType.PX4] = (2, 2),
        [ContainerType.PX2] = (1, 2)
    };

    public async Task<StorageInitializationResult> InitializeAsync(StorageInitializationRequest request)
    {
        ValidateRequest(request);

        if (await context.Containers.AnyAsync())
            throw new InvalidOperationException("Storage is already initialized.");

        var groups = new List<ContainerGroup>();
        var containersByType = new Dictionary<ContainerType, int>();
        var currentNumber = request.StartIndex;

        foreach (var cabinet in request.Cabinets.OrderBy(c => c.CabinetIndex))
        {
            foreach (var shelf in cabinet.Shelves.OrderBy(s => s.ShelfIndex))
            {
                var rowTypes = shelf.RowTypes.ToList();

                for (var groupRow = 1; groupRow <= rowTypes.Count; groupRow++)
                {
                    for (var groupColumn = 1; groupColumn <= cabinet.GroupColumnsCount; groupColumn++)
                    {
                        var type = rowTypes[groupRow - 1];
                        var group = new ContainerGroup(type, cabinet.CabinetIndex, shelf.ShelfIndex, groupRow, groupColumn);
                        var layout = LayoutMap[type];

                        for (var positionRow = 1; positionRow <= layout.rows; positionRow++)
                        {
                            for (var positionColumn = 1; positionColumn <= layout.columns; positionColumn++)
                            {
                                var container = new Container(
                                    type,
                                    currentNumber,
                                    new Location(
                                        cabinet.CabinetIndex,
                                        shelf.ShelfIndex,
                                        groupRow,
                                        groupColumn,
                                        positionRow,
                                        positionColumn));

                                container.InitializeSections();
                                group.AddContainer(container);
                                IncrementContainerCount(containersByType, type);
                                currentNumber++;
                            }
                        }

                        groups.Add(group);
                    }
                }
            }
        }

        await context.ContainerGroups.AddRangeAsync(groups);
        await context.SaveChangesAsync();

        var sectionsCount = groups
            .SelectMany(g => g.Containers)
            .Sum(container => container.Sections.Count);

        return new StorageInitializationResult
        {
            ContainersCreated = currentNumber - request.StartIndex,
            SectionsCreated = sectionsCount,
            ContainersByType = containersByType
        };
    }

    public Task<BsxFileMetadata> GetBsxFileMetadataAsync(string filePath)
    {
        var fileInfo = ValidateBsxFile(filePath);

        return Task.FromResult(new BsxFileMetadata
        {
            FileName = fileInfo.Name,
            FilePath = fileInfo.FullName,
            FileSizeBytes = fileInfo.Length
        });
    }

    public async Task<BsxFileProcessingResult> ProcessBsxFileAsync(BsxFileProcessingRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var fileInfo = ValidateBsxFile(request.FilePath);

        if (!await context.Containers.AnyAsync())
            throw new InvalidOperationException("Storage must be initialized before BSX processing can start.");

        var batchSize = request.BatchSize.GetValueOrDefault(DefaultBatchSize);
        if (batchSize <= 0)
            batchSize = DefaultBatchSize;

        var document = XDocument.Load(fileInfo.FullName);
        var matchingItems = document
            .Descendants("Item")
            .Where(IsCandidateItem)
            .ToList();

        var warnings = new List<string>();
        var processedItemCount = 0;
        var createdLotCount = 0;
        var updatedLotCount = 0;
        var logPath = Path.Combine(fileInfo.DirectoryName ?? string.Empty, $"{Path.GetFileNameWithoutExtension(fileInfo.Name)}.processing.log");
        var stopwatch = Stopwatch.StartNew();

        AppendLog(logPath, $"Starting BSX processing for '{fileInfo.FullName}'. Matching item count: {matchingItems.Count}.");

        for (var batchIndex = 0; batchIndex < matchingItems.Count; batchIndex += batchSize)
        {
            var batchNumber = (batchIndex / batchSize) + 1;
            var batch = matchingItems.Skip(batchIndex).Take(batchSize).ToList();
            var containerCache = new Dictionary<int, Container?>();
            var ensuredLots = new Dictionary<int, string>();

            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                foreach (var item in batch)
                {
                    if (!TryParseProcessingItem(item, out var parsedItem, out var warning))
                    {
                        AddWarning(warnings, logPath, warning ?? "The item could not be parsed and was skipped.");
                        continue;
                    }

                    var container = await GetContainerAsync(parsedItem.ContainerNumber, containerCache);
                    if (container is null)
                    {
                        AddWarning(warnings, logPath, $"Container #{parsedItem.ContainerNumber} was not found. Lot {parsedItem.LotId} was skipped.");
                        continue;
                    }

                    var section = SelectTargetSection(container);
                    if (section is null)
                    {
                        AddWarning(warnings, logPath, $"Container #{parsedItem.ContainerNumber} has no available sections. Lot {parsedItem.LotId} was skipped.");
                        continue;
                    }

                    var lotEnsureResult = await EnsureLotExistsAsync(parsedItem.LotId, parsedItem.ItemId, ensuredLots);
                    if (lotEnsureResult == LotEnsureResult.ItemMismatch)
                    {
                        AddWarning(warnings, logPath, $"Lot {parsedItem.LotId} already exists with a different ItemID. The item for container #{parsedItem.ContainerNumber} was skipped.");
                        continue;
                    }

                    if (lotEnsureResult == LotEnsureResult.Created)
                    {
                        createdLotCount++;
                        AppendLog(logPath, $"Created lot {parsedItem.LotId} for ItemID {parsedItem.ItemId}.");
                    }

                    var lotSection = await FindLotSectionAsync(parsedItem.LotId, section.Id);
                    if (lotSection is null)
                    {
                        context.LotSections.Add(new LotSection(parsedItem.LotId, section.Id, parsedItem.Quantity));
                        updatedLotCount++;
                        AppendLog(logPath, $"Assigned lot {parsedItem.LotId} to section {section.Id} with quantity {parsedItem.Quantity}.");
                    }
                    else
                    {
                        lotSection.AddQuantity(parsedItem.Quantity);
                        updatedLotCount++;
                        AppendLog(logPath, $"Merged quantity {parsedItem.Quantity} into lot {parsedItem.LotId} for section {section.Id}. New quantity: {lotSection.Quantity}.");
                    }

                    MarkItemProcessed(item);
                    processedItemCount++;
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                document.Save(fileInfo.FullName);
                AppendLog(logPath, $"Batch {batchNumber} committed successfully. Total processed items so far: {processedItemCount}.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                AppendLog(logPath, $"Batch {batchNumber} failed and was rolled back. Error: {ex.Message}");
                throw new InvalidOperationException($"BSX processing failed in batch {batchNumber}. {ex.Message}", ex);
            }
        }

        stopwatch.Stop();

        var summaryMessage = warnings.Count == 0
            ? $"BSX processing completed successfully. Processed {processedItemCount} item(s)."
            : $"BSX processing completed with warnings. Processed {processedItemCount} item(s) and recorded {warnings.Count} warning(s).";

        AppendLog(logPath, summaryMessage);

        return new BsxFileProcessingResult
        {
            Status = warnings.Count == 0 ? "completed" : "completed_with_warnings",
            ProcessedItemCount = processedItemCount,
            CreatedLotCount = createdLotCount,
            UpdatedLotCount = updatedLotCount,
            WarningCount = warnings.Count,
            ErrorCount = 0,
            ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
            SummaryMessage = summaryMessage,
            Warnings = warnings
        };
    }

    private static void ValidateRequest(StorageInitializationRequest request)
    {
        if (request.StartIndex <= 0)
            throw new ArgumentOutOfRangeException(nameof(request.StartIndex), "Start index must be a positive integer.");

        if (request.Cabinets.Count == 0)
            throw new InvalidOperationException("At least one cabinet configuration is required.");

        foreach (var cabinet in request.Cabinets)
        {
            if (cabinet.GroupColumnsCount < 1)
                throw new ArgumentOutOfRangeException(nameof(cabinet.GroupColumnsCount), $"Cabinet {cabinet.CabinetIndex} must have at least 1 container group column.");

            if (cabinet.Shelves.Count == 0)
                throw new InvalidOperationException($"Cabinet {cabinet.CabinetIndex} must contain at least one shelf.");

            foreach (var shelf in cabinet.Shelves)
            {
                if (shelf.RowTypes.Count == 0)
                    throw new InvalidOperationException($"Shelf {shelf.ShelfIndex} in cabinet {cabinet.CabinetIndex} must provide at least one container group row type.");

                foreach (var type in shelf.RowTypes)
                {
                    if (!LayoutMap.ContainsKey(type))
                        throw new InvalidOperationException("Unknown container type provided.");
                }
            }
        }
    }

    private static FileInfo ValidateBsxFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("A BSX file path is required.", nameof(filePath));

        var normalizedPath = Path.GetFullPath(filePath.Trim());

        if (!File.Exists(normalizedPath))
            throw new FileNotFoundException($"BSX file was not found: {normalizedPath}", normalizedPath);

        if (!string.Equals(Path.GetExtension(normalizedPath), ".bsx", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Only .bsx files are supported for storage initialization import.");

        return new FileInfo(normalizedPath);
    }

    private static bool IsCandidateItem(XElement item)
    {
        var remarks = (string?)item.Element("Remarks");
        if (string.IsNullOrWhiteSpace(remarks) || !ContainerRemarksRegex.IsMatch(remarks))
            return false;

        var processedValue = (string?)item.Element("Processed");
        return !string.Equals(processedValue, "true", StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryParseProcessingItem(XElement item, out ParsedBsxItem parsedItem, out string? warning)
    {
        parsedItem = default;
        warning = null;

        var remarks = (string?)item.Element("Remarks");
        var remarksMatch = remarks is null ? null : ContainerRemarksRegex.Match(remarks);
        if (remarksMatch is null || !remarksMatch.Success)
        {
            warning = "An item has invalid Remarks and was skipped.";
            return false;
        }

        if (!int.TryParse(remarksMatch.Groups["number"].Value, out var containerNumber))
        {
            warning = $"An item has an invalid container number in Remarks '{remarks}'.";
            return false;
        }

        if (!int.TryParse((string?)item.Element("LotID"), out var lotId))
        {
            warning = $"Container #{containerNumber} has an item with an invalid or missing LotID.";
            return false;
        }

        var itemId = (string?)item.Element("ItemID");
        if (string.IsNullOrWhiteSpace(itemId))
        {
            warning = $"Container #{containerNumber} has an item with an invalid or missing ItemID.";
            return false;
        }

        if (!int.TryParse((string?)item.Element("Qty"), out var quantity) || quantity <= 0)
        {
            warning = $"Container #{containerNumber} has an item with an invalid quantity.";
            return false;
        }

        parsedItem = new ParsedBsxItem(containerNumber, lotId, itemId, quantity);
        return true;
    }

    private async Task<Container?> GetContainerAsync(int containerNumber, IDictionary<int, Container?> containerCache)
    {
        if (containerCache.TryGetValue(containerNumber, out var cachedContainer))
            return cachedContainer;

        var container = await context.Containers
            .Include(value => value.Sections)
                .ThenInclude(section => section.LotSections)
            .FirstOrDefaultAsync(value => value.Number == containerNumber);

        containerCache[containerNumber] = container;
        return container;
    }

    private Section? SelectTargetSection(Container container)
    {
        var orderedSections = container.Sections.OrderBy(section => section.Index).ToList();
        var emptySection = orderedSections.FirstOrDefault(section => !SectionHasAssignedLots(section));

        return emptySection ?? orderedSections.OrderByDescending(section => section.Index).FirstOrDefault();
    }

    private bool SectionHasAssignedLots(Section section) =>
        section.LotSections.Any() || context.LotSections.Local.Any(lotSection => lotSection.SectionId == section.Id);

    private async Task<LotSection?> FindLotSectionAsync(int lotId, int sectionId)
    {
        var trackedLotSection = context.LotSections.Local.FirstOrDefault(lotSection =>
            lotSection.LotId == lotId && lotSection.SectionId == sectionId);

        if (trackedLotSection is not null)
            return trackedLotSection;

        return await context.LotSections.FirstOrDefaultAsync(lotSection =>
            lotSection.LotId == lotId && lotSection.SectionId == sectionId);
    }

    private async Task<LotEnsureResult> EnsureLotExistsAsync(int lotId, string itemId, IDictionary<int, string> ensuredLots)
    {
        if (ensuredLots.TryGetValue(lotId, out var ensuredItemId))
            return string.Equals(ensuredItemId, itemId, StringComparison.Ordinal) ? LotEnsureResult.Exists : LotEnsureResult.ItemMismatch;

        var existingLot = await context.Lots
            .AsNoTracking()
            .FirstOrDefaultAsync(lot => lot.Id == lotId);

        if (existingLot is not null)
        {
            ensuredLots[lotId] = existingLot.ItemId;
            return string.Equals(existingLot.ItemId, itemId, StringComparison.Ordinal) ? LotEnsureResult.Exists : LotEnsureResult.ItemMismatch;
        }

        await context.Database.ExecuteSqlInterpolatedAsync($@"
SET IDENTITY_INSERT [dbo].[Lots] ON;
INSERT INTO [dbo].[Lots] ([Id], [ItemId]) VALUES ({lotId}, {itemId});
SET IDENTITY_INSERT [dbo].[Lots] OFF;");

        ensuredLots[lotId] = itemId;
        return LotEnsureResult.Created;
    }

    private static void MarkItemProcessed(XElement item)
    {
        var processedElement = item.Element("Processed");
        if (processedElement is null)
            item.Add(new XElement("Processed", "true"));
        else
            processedElement.Value = "true";
    }

    private static void AppendLog(string logPath, string message)
    {
        var directory = Path.GetDirectoryName(logPath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        File.AppendAllText(logPath, $"[{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
    }

    private static void AddWarning(ICollection<string> warnings, string logPath, string warning)
    {
        warnings.Add(warning);
        AppendLog(logPath, $"Warning: {warning}");
    }

    private static void IncrementContainerCount(IDictionary<ContainerType, int> containersByType, ContainerType type)
    {
        if (!containersByType.TryGetValue(type, out var count))
            containersByType[type] = 1;
        else
            containersByType[type] = count + 1;
    }

    private readonly record struct ParsedBsxItem(int ContainerNumber, int LotId, string ItemId, int Quantity);

    private enum LotEnsureResult
    {
        Exists,
        Created,
        ItemMismatch
    }
}
