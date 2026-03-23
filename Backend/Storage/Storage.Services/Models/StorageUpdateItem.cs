namespace Storage.Services.Models;

public class StorageUpdateItem
{
    public required string ItemId { get; init; }

    public required int Qty { get; init; }

    public required string Remarks { get; init; }
}
