using FinancialEngine.Api.DTO;

namespace FinancialEngine.Api.Services;

public interface ITransactionService
{
    Task<TransactionResponse> ProcessEventAsync(TransactionEventRequest request, CancellationToken ct = default);

    Task<PagedResult<TransactionResponse>> GetStatementAsync(
        Guid accountId, int page, int pageSize, CancellationToken ct = default);
}