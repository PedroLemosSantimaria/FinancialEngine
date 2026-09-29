using FinancialEngine.Api.Models;

namespace FinancialEngine.Api.Repositories;

public interface ITransactionRepository
{
    Task<bool> ExistsAsync(Guid eventId, CancellationToken ct = default);
    Task AddAsync(Transaction transaction, CancellationToken ct = default);

    Task<(IReadOnlyList<Transaction> Items, int TotalCount)> GetPagedByAccountAsync(
        Guid accountId, int page, int pageSize, CancellationToken ct = default);
}