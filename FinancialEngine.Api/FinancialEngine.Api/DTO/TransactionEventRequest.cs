using System.ComponentModel.DataAnnotations;
using FinancialEngine.Api.Models.Enums;

namespace FinancialEngine.Api.DTO;

public class TransactionEventRequest
{
    [Required]
    public Guid EventId { get; set; }

    [Required]
    public Guid AccountId { get; set; }

    [Required]
    public TransactionType Type { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero.")]
    public decimal Amount { get; set; }

    [Required]
    public DateTime OccurredAt { get; set; }
}