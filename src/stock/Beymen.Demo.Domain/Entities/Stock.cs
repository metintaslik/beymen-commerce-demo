using Beymen.Demo.Domain.Exceptions;

namespace Beymen.Demo.Domain.Entities;

public class Stock : BaseEntity
{
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }

    private Stock() { }

    public Stock(Guid productId, int quantity)
    {
        ProductId = productId;
        Quantity = quantity;

        ValidateState();
    }

    public void UpdateQuantity(int newQuantity) => Quantity = newQuantity;

    public void ValidateState()
    {
        StockValidateStateException.ThrowIfProductEmpty(ProductId);
        StockValidateStateException.ThrowIfQuantityNegative(Quantity);
    }
}