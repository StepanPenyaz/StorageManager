using Storage.Domain.Enums;

namespace Storage.Domain.Models;

public class Container(ContainerType type)
{
    private readonly List<Section> _sections = [];

    public IReadOnlyCollection<Section> Sections => _sections;

    public int Id { get; private set; }

    public ContainerType Type { get; private set; } = type;

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