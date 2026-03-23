using Storage.Domain.Enums;
using Storage.Domain.Models;

namespace Storage.Tests;

public class ContainerGroupTests
{
    private static Location GroupLocation(int cabinet, int shelf, int groupRow, int groupColumn) =>
        new(cabinet, shelf, groupRow, groupColumn, 1, 1);

    [Fact]
    public void AddContainer_NullContainer_ThrowsArgumentNullException()
    {
        var group = new ContainerGroup(ContainerType.PX6, 1, 1, 1, 1);

        Assert.Throws<ArgumentNullException>(() => group.AddContainer(null!));
    }

    [Fact]
    public void AddContainer_WrongContainerType_ThrowsInvalidOperationException()
    {
        var group = new ContainerGroup(ContainerType.PX6, 1, 1, 1, 1);
        var container = new Container(ContainerType.PX12, 1, GroupLocation(1, 1, 1, 1));

        Assert.Throws<InvalidOperationException>(() => group.AddContainer(container));
    }

    [Fact]
    public void AddContainer_WrongCabinet_ThrowsInvalidOperationException()
    {
        var group = new ContainerGroup(ContainerType.PX6, 1, 1, 1, 1);
        var container = new Container(ContainerType.PX6, 1, GroupLocation(2, 1, 1, 1));

        Assert.Throws<InvalidOperationException>(() => group.AddContainer(container));
    }

    [Fact]
    public void AddContainer_WrongShelf_ThrowsInvalidOperationException()
    {
        var group = new ContainerGroup(ContainerType.PX6, 1, 1, 1, 1);
        var container = new Container(ContainerType.PX6, 1, GroupLocation(1, 2, 1, 1));

        Assert.Throws<InvalidOperationException>(() => group.AddContainer(container));
    }

    [Fact]
    public void AddContainer_WrongGroupRow_ThrowsInvalidOperationException()
    {
        var group = new ContainerGroup(ContainerType.PX6, 1, 1, 1, 1);
        var container = new Container(ContainerType.PX6, 1, GroupLocation(1, 1, 2, 1));

        Assert.Throws<InvalidOperationException>(() => group.AddContainer(container));
    }

    [Fact]
    public void AddContainer_WrongGroupColumn_ThrowsInvalidOperationException()
    {
        var group = new ContainerGroup(ContainerType.PX6, 1, 1, 1, 1);
        var container = new Container(ContainerType.PX6, 1, GroupLocation(1, 1, 1, 2));

        Assert.Throws<InvalidOperationException>(() => group.AddContainer(container));
    }

    [Fact]
    public void AddContainer_MatchingTypeAndLocation_AddsContainer()
    {
        var group = new ContainerGroup(ContainerType.PX6, 1, 1, 1, 1);
        var container = new Container(ContainerType.PX6, 1, GroupLocation(1, 1, 1, 1));

        group.AddContainer(container);

        Assert.Single(group.Containers);
    }

    [Fact]
    public void AddContainer_MultipleValidContainers_AddsAll()
    {
        var group = new ContainerGroup(ContainerType.PX6, 1, 1, 1, 1);
        var container1 = new Container(ContainerType.PX6, 1, GroupLocation(1, 1, 1, 1));
        var container2 = new Container(ContainerType.PX6, 2, GroupLocation(1, 1, 1, 1));

        group.AddContainer(container1);
        group.AddContainer(container2);

        Assert.Equal(2, group.Containers.Count);
    }
}
