
namespace Beymen.Demo.Domain.Entities;

public class Order : BaseEntity<Guid>
{
    public int? TotalQuantityCount { get; set; }

    public ICollection<OrderItem>? OrderItems { get; set; } = [];

    private Order() { }

    protected Order(Guid id, ICollection<OrderItem>? orderItems, int totalQuantityCount = 0)
    {
        Id = id;
        OrderItems = orderItems;
        TotalQuantityCount = totalQuantityCount;
    }

    public static Order Create() => new(Guid.NewGuid(), []);
}