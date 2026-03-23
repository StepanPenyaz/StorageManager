namespace Storage.Api.Models;

public class ProcessOrdersRequest
{
    public required string IncomingOrdersFolder { get; init; }

    public required string ProcessedOrdersFolder { get; init; }
}
