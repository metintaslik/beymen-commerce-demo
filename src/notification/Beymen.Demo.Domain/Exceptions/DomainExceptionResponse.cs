namespace Beymen.Demo.Domain.Exceptions
{
    public record DomainExceptionResponse(string Status, string Message, object? Details);
}