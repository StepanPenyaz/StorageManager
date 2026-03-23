namespace Storage.Domain.Models;

public class LotSection(int lotId, int sectionId, int quantity)
{
    public int LotId { get; private set; } = lotId;

    public int SectionId { get; private set; } = sectionId;

    public int Quantity { get; private set; } = quantity;

    public void AddQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new InvalidOperationException("Quantity to add must be greater than zero.");

        Quantity += quantity;
    }

    public void SubtractQuantity(int quantity) =>
        Quantity = Quantity >= quantity
            ? Quantity - quantity
            : throw new InvalidOperationException($"Cannot subtract {quantity} from quantity {Quantity}.");
}
