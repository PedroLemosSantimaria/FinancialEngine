namespace FinancialEngine.Api.Exceptions;

public class AccountNotFoundException : Exception
{
    public AccountNotFoundException(Guid accountId)
        : base($"Conta '{accountId}' não encontrada.")
    {
    }
}