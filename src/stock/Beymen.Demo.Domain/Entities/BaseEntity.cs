namespace Beymen.Demo.Domain.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; } = false;

    public void SetUpdatedAt() => UpdatedAt = DateTime.UtcNow;

    public void MarkAsDeleted()
    {
        IsDeleted = true;
        SetUpdatedAt();
    }
}