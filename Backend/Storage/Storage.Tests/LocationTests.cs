using Storage.Domain.Models;

namespace Storage.Tests;

public class LocationTests
{
    [Fact]
    public void Constructor_WithAllZeroValues_CreatesLocation()
    {
        var location = new Location(0, 0, 0, 0, 0, 0);

        Assert.Equal(0, location.Cabinet);
        Assert.Equal(0, location.Shelf);
        Assert.Equal(0, location.GroupRow);
        Assert.Equal(0, location.GroupColumn);
        Assert.Equal(0, location.PositionRow);
        Assert.Equal(0, location.PositionColumn);
    }

    [Fact]
    public void Constructor_WithPositiveValues_CreatesLocation()
    {
        var location = new Location(1, 2, 3, 4, 5, 6);

        Assert.Equal(1, location.Cabinet);
        Assert.Equal(2, location.Shelf);
        Assert.Equal(3, location.GroupRow);
        Assert.Equal(4, location.GroupColumn);
        Assert.Equal(5, location.PositionRow);
        Assert.Equal(6, location.PositionColumn);
    }

    [Fact]
    public void Constructor_WithNegativeCabinet_ThrowsArgumentOutOfRangeException() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new Location(-1, 0, 0, 0, 0, 0));

    [Fact]
    public void Constructor_WithNegativeShelf_ThrowsArgumentOutOfRangeException() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new Location(0, -1, 0, 0, 0, 0));

    [Fact]
    public void Constructor_WithNegativeGroupRow_ThrowsArgumentOutOfRangeException() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new Location(0, 0, -1, 0, 0, 0));

    [Fact]
    public void Constructor_WithNegativeGroupColumn_ThrowsArgumentOutOfRangeException() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new Location(0, 0, 0, -1, 0, 0));

    [Fact]
    public void Constructor_WithNegativePositionRow_ThrowsArgumentOutOfRangeException() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new Location(0, 0, 0, 0, -1, 0));

    [Fact]
    public void Constructor_WithNegativePositionColumn_ThrowsArgumentOutOfRangeException() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new Location(0, 0, 0, 0, 0, -1));
}
