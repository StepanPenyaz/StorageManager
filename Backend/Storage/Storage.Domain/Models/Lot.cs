namespace Storage.Domain.Models;

public class Lot(int itemId)
{
    private readonly List<LotSection> _lotSections = [];

    public int Id { get; private set; }

    public int ItemId { get; private set; } = itemId;

    public IReadOnlyCollection<LotSection> LotSections => _lotSections;
}
