namespace Storage.Domain.Models;

public class LotSection(int lotId, int sectionId, int quantity)
{
    public int LotId { get; private set; } = lotId;

    public int SectionId { get; private set; } = sectionId;

    public int Quantity { get; private set; } = quantity;
}
