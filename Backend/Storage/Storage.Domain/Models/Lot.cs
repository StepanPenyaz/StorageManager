namespace Storage.Domain.Models;

public class Lot(string itemId)
{
    private readonly List<LotSection> _lotSections = [];

    public int Id { get; private set; }

    public string ItemId { get; private set; } = itemId;

    public IReadOnlyCollection<LotSection> LotSections => _lotSections;
}
