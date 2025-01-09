namespace Beymen.Demo.Domain.Exceptions;

public class NotificationNotFoundException(Guid id) : DomainException($"Notification with ID {id} was not found.")
{
    public Guid NotificationId { get; } = id;
}
