using Microsoft.AspNetCore.Mvc;
using Storage.Api.Models;
using Storage.Services.Interfaces;

namespace Storage.Api.Controllers;

[ApiController]
[Route("api/storage")]
public class StorageController(
    IStorageViewService storageViewService,
    IOrderProcessingService orderProcessingService,
    IStorageUpdateProcessingService storageUpdateProcessingService) : ControllerBase
{
    [HttpGet("cabinets")]
    public async Task<IActionResult> GetCabinets() =>
        Ok(await storageViewService.GetCabinetNumbersAsync());

    [HttpGet("cabinets/{cabinetNumber:int}/containers")]
    public async Task<IActionResult> GetCabinetContainers(int cabinetNumber)
    {
        if (cabinetNumber < 0)
            return BadRequest("Cabinet number must be a non-negative integer.");

        var containers = await storageViewService.GetCabinetContainersAsync(cabinetNumber);

        return containers.Count == 0
            ? NotFound($"No containers with empty sections found for cabinet {cabinetNumber}.")
            : Ok(containers.Select(c => new ContainerDto
            {
                Number = c.Number,
                Type = c.Type.ToString(),
                Shelf = c.Shelf,
                GroupRow = c.GroupRow,
                GroupColumn = c.GroupColumn,
                PositionRow = c.PositionRow,
                PositionColumn = c.PositionColumn,
                TotalSections = c.TotalSections,
                EmptySections = c.EmptySections
            }));
    }

    [HttpPost("orders/process")]
    public async Task<IActionResult> ProcessOrders([FromBody] ProcessOrdersRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.IncomingOrdersFolder))
            return BadRequest("Incoming orders folder path is required.");

        if (string.IsNullOrWhiteSpace(request.ProcessedOrdersFolder))
            return BadRequest("Processed orders folder path is required.");

        var result = await orderProcessingService.ProcessOrdersAsync(
            request.IncomingOrdersFolder,
            request.ProcessedOrdersFolder);

        return Ok(new ProcessingResultDto
        {
            FilesProcessed = result.FilesProcessed,
            ItemsProcessed = result.ItemsProcessed,
            WarningCount = result.WarningCount,
            ErrorCount = result.ErrorCount,
            Warnings = result.Warnings
        });
    }

    [HttpPost("updates/process")]
    public async Task<IActionResult> ProcessStorageUpdates([FromBody] ProcessStorageUpdatesRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.IncomingStorageUpdatesFolder))
            return BadRequest("Incoming storage updates folder path is required.");

        if (string.IsNullOrWhiteSpace(request.ProcessedStorageUpdatesFolder))
            return BadRequest("Processed storage updates folder path is required.");

        var result = await storageUpdateProcessingService.ProcessStorageUpdatesAsync(
            request.IncomingStorageUpdatesFolder,
            request.ProcessedStorageUpdatesFolder);

        return Ok(new ProcessingResultDto
        {
            FilesProcessed = result.FilesProcessed,
            ItemsProcessed = result.ItemsProcessed,
            WarningCount = result.WarningCount,
            ErrorCount = result.ErrorCount,
            Warnings = result.Warnings
        });
    }
}
