using FinancialEngine.Api.Models.Enums;

namespace FinancialEngine.Api.Models;

public class Transaction
{
    public Guid EventId { get; set; }      
    public Guid AccountId { get; set; }
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public DateTime OccurredAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public Account Account { get; set; } = null!;
}