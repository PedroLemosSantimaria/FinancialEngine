using FinancialEngine.Api.Exceptions;
using FinancialEngine.Api.Models.Enums;

namespace FinancialEngine.Api.Models;

public class Account
{
    public Guid Id { get; set; }
    public string Owner { get; set; } = string.Empty;
    public decimal Balance { get; private set; }
    public DateTime CreatedAt { get; set; }

    
    public uint Version { get; set; }

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    
    private Account() { }

    public Account(Guid id, string owner, decimal initialBalance)
    {
        Id = id;
        Owner = owner;
        Balance = initialBalance;
        CreatedAt = DateTime.UtcNow;
    }

 
    public Transaction Apply(Guid eventId, TransactionType type, decimal amount, DateTime occurredAt)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "O valor da transação deve ser positivo.");

        Balance = type switch
        {
            TransactionType.Credit => Balance + amount,
            TransactionType.Debit => Balance - amount < 0
                ? throw new InsufficientBalanceException(Id, Balance, amount)
                : Balance - amount,
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };

        return new Transaction
        {
            EventId = eventId,
            AccountId = Id,
            Type = type,
            Amount = amount,
            BalanceAfter = Balance,
            OccurredAt = occurredAt,
            CreatedAt = DateTime.UtcNow
        };
    }
}