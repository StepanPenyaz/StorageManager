using Storage.Domain.Models;

namespace Storage.Tests;

public class LotSectionTests
{
    [Fact]
    public void AddQuantity_PositiveAmount_IncreasesQuantity()
    {
        var lotSection = new LotSection(1, 1, 10);

        lotSection.AddQuantity(5);

        Assert.Equal(15, lotSection.Quantity);
    }

    [Fact]
    public void AddQuantity_ZeroAmount_ThrowsInvalidOperationException() =>
        Assert.Throws<InvalidOperationException>(() => new LotSection(1, 1, 10).AddQuantity(0));

    [Fact]
    public void AddQuantity_NegativeAmount_ThrowsInvalidOperationException() =>
        Assert.Throws<InvalidOperationException>(() => new LotSection(1, 1, 10).AddQuantity(-1));

    [Fact]
    public void SubtractQuantity_LessThanCurrent_DecreasesQuantity()
    {
        var lotSection = new LotSection(1, 1, 10);

        lotSection.SubtractQuantity(4);

        Assert.Equal(6, lotSection.Quantity);
    }

    [Fact]
    public void SubtractQuantity_ExactAmount_ReducesToZero()
    {
        var lotSection = new LotSection(1, 1, 10);

        lotSection.SubtractQuantity(10);

        Assert.Equal(0, lotSection.Quantity);
    }

    [Fact]
    public void SubtractQuantity_MoreThanCurrent_ThrowsInvalidOperationException() =>
        Assert.Throws<InvalidOperationException>(() => new LotSection(1, 1, 5).SubtractQuantity(6));
}
