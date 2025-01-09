namespace Beymen.Demo.Domain.Exceptions;

public class InvalidNotificationException(string message) : DomainException(message)
{
}