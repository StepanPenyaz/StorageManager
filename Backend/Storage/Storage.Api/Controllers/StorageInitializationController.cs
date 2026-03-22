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
            return BadRequest(ex.Message);
        }
    }

    private static StorageInitializationRequest MapRequest(StorageInitializationDto request) =>
        new()
        {
            StartIndex = request.StartIndex,
            BsxFileName = request.BsxFileName,
            BsxFileContentBase64 = request.BsxFileContentBase64,
            Cabinets = request.Cabinets
                .OrderBy(c => c.CabinetIndex)
                .Select(c => new CabinetConfiguration
                {
                    CabinetIndex = c.CabinetIndex,
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
