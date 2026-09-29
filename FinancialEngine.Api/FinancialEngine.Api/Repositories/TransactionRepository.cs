using FinancialEngine.Api.Data;
using FinancialEngine.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FinancialEngine.Api.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly AppDbContext _context;

    public TransactionRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<bool> ExistsAsync(Guid eventId, CancellationToken ct = default) =>
        _context.Transactions.AnyAsync(t => t.EventId == eventId, ct);

    public Task AddAsync(Transaction transaction, CancellationToken ct = default)
    {
        _context.Transactions.Add(transaction);
        return Task.CompletedTask;
    }

    public async Task<(IReadOnlyList<Transaction> Items, int TotalCount)> GetPagedByAccountAsync(
        Guid accountId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _context.Transactions
            .AsNoTracking()
            .Where(t => t.AccountId == accountId)
            .OrderByDescending(t => t.OccurredAt);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}