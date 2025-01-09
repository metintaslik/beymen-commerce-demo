using Beymen.Demo.Domain.Enums;
using Beymen.Demo.Domain.Exceptions;

namespace Beymen.Demo.Domain.Entities;

public class Notification
{
    public Guid Id { get; private set; }
    public string? Title { get; private set; }
    public string? Content { get; private set; }
    public string? Recipient { get; private set; }
    public NotificationType Type { get; private set; }
    public NotificationStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public bool IsDeleted { get; set; }

    private Notification() { } // For EF Core

    private Notification(string title, string content, NotificationType type, string recipient)
    {
        Id = Guid.NewGuid();
        Title = title;
        Content = content;
        Type = type;
        Status = NotificationStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        Recipient = recipient;
        IsDeleted = false;
    }

    public static Notification Create(string title, string content, NotificationType type, string recipient)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(title))
            errors.Add("Title cannot be empty");

        if (string.IsNullOrWhiteSpace(content))
            errors.Add("Content cannot be empty");

        if (errors.Count != 0)
            throw new NotificationValidationException("Invalid notification data", errors);

        return new Notification(title, content, type, recipient);
    }

    public void MarkAsSent()
    {
        CheckStatus();

        Status = NotificationStatus.Sent;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed()
    {
        CheckStatus();

        Status = NotificationStatus.Failed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsDelete()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }

    private void CheckStatus()
    {
        if (Status != NotificationStatus.Pending)
            throw new InvalidNotificationException(
                $"Cannot mark notification as sent. Current status: {Status}");
    }
}
