using FinancialEngine.Api.Data;
using FinancialEngine.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FinancialEngine.Api.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly AppDbContext _context;

    public AccountRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Account?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _context.Accounts.FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<IReadOnlyList<Account>> GetAllAsync(CancellationToken ct = default) =>
        await _context.Accounts.AsNoTracking().OrderBy(a => a.Owner).ToListAsync(ct);
}