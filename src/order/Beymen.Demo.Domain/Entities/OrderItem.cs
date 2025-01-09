namespace Beymen.Demo.Domain.Entities;

public class OrderItem : BaseEntity<int>
{
    public Guid OrderId { get; private set; }

    public Guid ProductId { get; private set; }

    public int Quantity { get; private set; }

    public Order? Order { get; set; }

    private OrderItem() { }

    public OrderItem(Guid orderId, Guid productId, int quantity)
    {
        OrderId = orderId;
        ProductId = productId;
        Quantity = quantity;
        SetCreatedAt();
    }
}