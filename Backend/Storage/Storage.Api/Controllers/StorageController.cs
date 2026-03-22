using Microsoft.AspNetCore.Mvc;
using Storage.Api.Models;
using Storage.Services.Interfaces;

namespace Storage.Api.Controllers;

[ApiController]
[Route("api/storage")]
public class StorageController(IStorageViewService storageViewService) : ControllerBase
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
}
