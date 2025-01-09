using Beymen.Demo.Domain.Enums;

namespace Beymen.Demo.Application.DTOs;

public record NotificationDto(
    string Title,
    string Content,
    NotificationType Type,
    string Recipient
);