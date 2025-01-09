namespace Beymen.Demo.Domain.Exceptions;

public class NotificationValidationException(string message, IEnumerable<string> errors) : DomainException(message)
{
    public IReadOnlyList<string> Errors { get; } = errors?.ToList().AsReadOnly() ?? new List<string>().AsReadOnly();
}