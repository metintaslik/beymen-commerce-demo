namespace Beymen.Demo.Domain.Entities;

public abstract class BaseEntity
{
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; } = false;

    public void SetCreatedAt() => CreatedAt = DateTime.UtcNow;

    public void SetUpdatedAt() => UpdatedAt = DateTime.UtcNow;

    public void MarkAsDeleted()
    {
        IsDeleted = true;
        SetUpdatedAt();
    }
}

public abstract class BaseEntity<TPropertyType> : BaseEntity
{
    public TPropertyType? Id { get; protected set; }
}