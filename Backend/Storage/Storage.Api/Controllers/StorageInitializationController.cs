using Microsoft.AspNetCore.Mvc;
using Storage.Api.Models;
using Storage.Domain.Enums;
using Storage.Services.Interfaces;
using Storage.Services.Models;

namespace Storage.Api.Controllers;

[ApiController]
[Route("api/storage/init")]
public class StorageInitializationController(IStorageInitializationService initializationService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Initialize([FromBody] StorageInitializationDto request)
    {
        try
        {
            var result = await initializationService.InitializeAsync(MapRequest(request));

            return Ok(new StorageInitializationResultDto
            {
                ContainersCreated = result.ContainersCreated,
                SectionsCreated = result.SectionsCreated,
                ContainersByType = result.ContainersByType
            });
        }
        catch (Exception ex) when (ex is ArgumentOutOfRangeException or InvalidOperationException)
        {
            return BadRequest(new { error = "Invalid storage initialization request.", detail = ex.Message });
        }
    }

    [HttpPost("bsx/metadata")]
    public async Task<IActionResult> GetBsxMetadata([FromBody] BsxFilePathDto request)
    {
        try
        {
            var result = await initializationService.GetBsxFileMetadataAsync(request.FilePath);

            return Ok(new BsxFileMetadataDto
            {
                FileName = result.FileName,
                FilePath = result.FilePath,
                FileSizeBytes = result.FileSizeBytes
            });
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or IOException or UnauthorizedAccessException)
        {
            return BadRequest(new { error = "Invalid BSX file request.", detail = ex.Message });
        }
    }

    [HttpPost("bsx/process")]
    public async Task<IActionResult> ProcessBsxFile([FromBody] BsxFileProcessingDto request)
    {
        try
        {
            var result = await initializationService.ProcessBsxFileAsync(new BsxFileProcessingRequest
            {
                FilePath = request.FilePath,
                BatchSize = request.BatchSize
            });

            return Ok(new BsxFileProcessingResultDto
            {
                Status = result.Status,
                ProcessedItemCount = result.ProcessedItemCount,
                CreatedLotCount = result.CreatedLotCount,
                UpdatedLotCount = result.UpdatedLotCount,
                WarningCount = result.WarningCount,
                ErrorCount = result.ErrorCount,
                ElapsedMilliseconds = result.ElapsedMilliseconds,
                SummaryMessage = result.SummaryMessage,
                Warnings = result.Warnings
            });
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or IOException or UnauthorizedAccessException)
        {
            return BadRequest(new { error = "BSX processing failed.", detail = ex.Message });
        }
    }

    private static StorageInitializationRequest MapRequest(StorageInitializationDto request) =>
        new()
        {
            StartIndex = request.StartIndex,
            Cabinets = request.Cabinets
                .OrderBy(c => c.CabinetIndex)
                .Select(c => new CabinetConfiguration
                {
                    CabinetIndex = c.CabinetIndex,
                    GroupColumnsCount = c.GroupColumnsCount,
                    Shelves = c.Shelves
                        .OrderBy(s => s.ShelfIndex)
                        .Select(s => new ShelfConfiguration
                        {
                            ShelfIndex = s.ShelfIndex,
                            RowTypes = s.RowTypes.Select(ParseType).ToList()
                        })
                        .ToList()
                })
                .ToList()
        };

    private static ContainerType ParseType(string value) =>
        Enum.TryParse<ContainerType>(value, out var type)
            ? type
            : throw new InvalidOperationException($"Unknown container type: {value}");
}
