using FinancialEngine.Api.Models;
using FinancialEngine.Api.Models.Enums;

namespace FinancialEngine.Api.DTO;

public class TransactionResponse
{
    public Guid EventId { get; set; }
    public Guid AccountId { get; set; }
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public DateTime OccurredAt { get; set; }

    public static TransactionResponse FromEntity(Transaction t) => new()
    {
        EventId = t.EventId,
        AccountId = t.AccountId,
        Type = t.Type,
        Amount = t.Amount,
        BalanceAfter = t.BalanceAfter,
        OccurredAt = t.OccurredAt
    };
}