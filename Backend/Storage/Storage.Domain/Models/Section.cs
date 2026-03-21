namespace Storage.Domain.Models;

public class Section(int index)
{
    public int Id { get; private set; }

    public int Index { get; private set; } = index;

    public int ContainerId { get; private set; }
}