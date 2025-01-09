using System.ComponentModel.DataAnnotations;

namespace Beymen.Demo.Domain.Exceptions;

public class StockValidateStateException : ValidationException
{

    public static void ThrowIfProductEmpty(Guid ProductId)
    {
        if (ProductId == Guid.Empty)
        {
            Throw(nameof(ProductId));
        }
    }

    public static void ThrowIfQuantityNegative(int Quantity)
    {
        if (Quantity < 0)
        {
            Throw(nameof(Quantity) + " cannot be negative.");
        }
    }

    private static void Throw(string paramName) => throw new ValidationException(paramName + " cannot be empty.");
}