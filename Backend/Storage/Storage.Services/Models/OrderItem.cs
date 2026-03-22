namespace Storage.Services.Models;

public class OrderItem
{
    public required int LotId { get; init; }

    public required int Qty { get; init; }

    public required string Remarks { get; init; }
}
