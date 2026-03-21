using Storage.Domain.Enums;

namespace Storage.Domain.Models;

public class Container(ContainerType type, int number, Location location)
{
    private readonly List<Section> _sections = [];

    private Container() : this(default, default, new Location(default, default, default, default, default, default))
    {
    }

    public IReadOnlyCollection<Section> Sections => _sections;

    public int Id { get; private set; }

    public int Number { get; private set; } = number;

    public ContainerType Type { get; private set; } = type;

    public Location Location { get; private set; } = location;

    public int ContainerGroupId { get; private set; }

    public void InitializeSections()
    {
        if (_sections.Count != 0)
        {
            return;
        }

        var count = Type switch
        {
            ContainerType.PX12 => 3,
            _ => 1
        };

        for (int i = 1; i <= count; i++)
        {
            _sections.Add(new Section(i));
        }
    }
}
