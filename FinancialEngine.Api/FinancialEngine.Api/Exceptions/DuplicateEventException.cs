namespace FinancialEngine.Api.Exceptions;

public class DuplicateEventException : Exception
{
    public DuplicateEventException(Guid eventId)
        : base($"O evento '{eventId}' já foi processado anteriormente.")
    {
    }
}