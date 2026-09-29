using FinancialEngine.Api.Models;

namespace FinancialEngine.Api.DTO;

public class AccountResponse
{
    public Guid Id { get; set; }
    public string Owner { get; set; } = string.Empty;
    public decimal Balance { get; set; }

    public static AccountResponse FromEntity(Account a) => new()
    {
        Id = a.Id,
        Owner = a.Owner,
        Balance = a.Balance
    };
}