namespace FinancialEngine.Api.Exceptions;

public class InsufficientBalanceException : Exception
{
    public InsufficientBalanceException(Guid accountId, decimal balance, decimal amount)
        : base($"Saldo insuficiente na conta '{accountId}'. Saldo atual: {balance}, valor solicitado: {amount}.")
    {
    }
}