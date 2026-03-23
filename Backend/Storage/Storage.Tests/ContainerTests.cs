using Storage.Domain.Enums;
using Storage.Domain.Models;

namespace Storage.Tests;

public class ContainerTests
{
    private static Container CreateContainer(ContainerType type) =>
        new(type, 1, new Location(1, 1, 1, 1, 1, 1));

    [Fact]
    public void InitializeSections_PX12_CreatesThreeSections()
    {
        var container = CreateContainer(ContainerType.PX12);

        container.InitializeSections();

        Assert.Equal(3, container.Sections.Count);
    }

    [Fact]
    public void InitializeSections_PX12_SectionsHaveCorrectIndices()
    {
        var container = CreateContainer(ContainerType.PX12);

        container.InitializeSections();

        var indices = container.Sections.Select(s => s.Index).Order().ToList();
        Assert.Equal([1, 2, 3], indices);
    }

    [Fact]
    public void InitializeSections_PX6_CreatesOneSection()
    {
        var container = CreateContainer(ContainerType.PX6);

        container.InitializeSections();

        Assert.Single(container.Sections);
    }

    [Fact]
    public void InitializeSections_PX4_CreatesOneSection()
    {
        var container = CreateContainer(ContainerType.PX4);

        container.InitializeSections();

        Assert.Single(container.Sections);
    }

    [Fact]
    public void InitializeSections_PX2_CreatesOneSection()
    {
        var container = CreateContainer(ContainerType.PX2);

        container.InitializeSections();

        Assert.Single(container.Sections);
    }

    [Fact]
    public void InitializeSections_CalledTwice_SectionsAreNotDuplicated()
    {
        var container = CreateContainer(ContainerType.PX12);

        container.InitializeSections();
        container.InitializeSections();

        Assert.Equal(3, container.Sections.Count);
    }

    [Fact]
    public void InitializeSections_SingleSection_HasIndexOne()
    {
        var container = CreateContainer(ContainerType.PX6);

        container.InitializeSections();

        Assert.Equal(1, container.Sections.Single().Index);
    }
}
