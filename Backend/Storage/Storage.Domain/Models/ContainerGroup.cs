using Storage.Domain.Enums;

namespace Storage.Domain.Models;

public class ContainerGroup(ContainerType type, int cabinet, int shelf, int groupRow, int groupColumn)
{
    private readonly List<Container> _containers = [];

    public int Id { get; private set; }

    public ContainerType Type { get; private set; } = type;

    public int Cabinet { get; private set; } = cabinet;

    public int Shelf { get; private set; } = shelf;

    public int GroupRow { get; private set; } = groupRow;

    public int GroupColumn { get; private set; } = groupColumn;

    public IReadOnlyCollection<Container> Containers => _containers;

    public void AddContainer(Container container)
    {
        ArgumentNullException.ThrowIfNull(container);

        if (container.Type != Type)
            throw new InvalidOperationException("Container type must match container group type.");

        if (container.Location.Cabinet != Cabinet ||
            container.Location.Shelf != Shelf ||
            container.Location.GroupRow != GroupRow ||
            container.Location.GroupColumn != GroupColumn)
            throw new InvalidOperationException("Container location must match container group coordinates.");

        _containers.Add(container);
    }
}
