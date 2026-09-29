using FinancialEngine.Api.Models;

namespace FinancialEngine.Api.Repositories;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Account>> GetAllAsync(CancellationToken ct = default);
}