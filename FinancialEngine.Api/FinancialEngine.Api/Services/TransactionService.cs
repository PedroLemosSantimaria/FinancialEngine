using FinancialEngine.Api.Data;
using FinancialEngine.Api.DTO;
using FinancialEngine.Api.Exceptions;
using FinancialEngine.Api.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinancialEngine.Api.Services;

public class TransactionService : ITransactionService
{
    private readonly AppDbContext _context;
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(
        AppDbContext context,
        IAccountRepository accountRepository,
        ITransactionRepository transactionRepository,
        ILogger<TransactionService> logger)
    {
        _context = context;
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _logger = logger;
    }

    public async Task<TransactionResponse> ProcessEventAsync(TransactionEventRequest request, CancellationToken ct = default)
    {
        
        if (await _transactionRepository.ExistsAsync(request.EventId, ct))
            throw new DuplicateEventException(request.EventId);

        var account = await _accountRepository.GetByIdAsync(request.AccountId, ct)
            ?? throw new AccountNotFoundException(request.AccountId);

        await using var dbTransaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            var transaction = account.Apply(request.EventId, request.Type, request.Amount, request.OccurredAt);
            await _transactionRepository.AddAsync(transaction, ct);

            await _context.SaveChangesAsync(ct);
            await dbTransaction.CommitAsync(ct);

            return TransactionResponse.FromEntity(transaction);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            
            await dbTransaction.RollbackAsync(ct);
            _logger.LogWarning("Evento duplicado detectado na gravação: {EventId}", request.EventId);
            throw new DuplicateEventException(request.EventId);
        }
        catch
        {
            await dbTransaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<PagedResult<TransactionResponse>> GetStatementAsync(
        Guid accountId, int page, int pageSize, CancellationToken ct = default)
    {
        if (await _accountRepository.GetByIdAsync(accountId, ct) is null)
            throw new AccountNotFoundException(accountId);

        var (items, totalCount) = await _transactionRepository.GetPagedByAccountAsync(accountId, page, pageSize, ct);

        return new PagedResult<TransactionResponse>
        {
            Items = items.Select(TransactionResponse.FromEntity).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    private static bool IsUniqueViolation(DbUpdateException ex) =>
        ex.InnerException is Npgsql.PostgresException pgEx && pgEx.SqlState == "23505";
}