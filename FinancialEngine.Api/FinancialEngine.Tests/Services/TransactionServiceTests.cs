using FinancialEngine.Api.Data;
using FinancialEngine.Api.DTO;
using FinancialEngine.Api.Exceptions;
using FinancialEngine.Api.Models;
using FinancialEngine.Api.Models.Enums;
using FinancialEngine.Api.Repositories;
using FinancialEngine.Api.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FinancialEngine.Tests.Services;

public class TransactionServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IAccountRepository> _accountRepositoryMock = new();
    private readonly Mock<ITransactionRepository> _transactionRepositoryMock = new();
    private readonly TransactionService _sut;

    public TransactionServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context = new AppDbContext(options);

        _sut = new TransactionService(
            _context,
            _accountRepositoryMock.Object,
            _transactionRepositoryMock.Object,
            new LoggerFactory().CreateLogger<TransactionService>());
    }

    [Fact]
    public async Task ProcessEventAsync_With_New_Event_Should_Apply_Transaction()
    {
        var account = new Account(Guid.NewGuid(), "Teste", 100m);
        var request = new TransactionEventRequest
        {
            EventId = Guid.NewGuid(),
            AccountId = account.Id,
            Type = TransactionType.Credit,
            Amount = 50m,
            OccurredAt = DateTime.UtcNow
        };

        _transactionRepositoryMock.Setup(r => r.ExistsAsync(request.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _accountRepositoryMock.Setup(r => r.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var result = await _sut.ProcessEventAsync(request);

        result.BalanceAfter.Should().Be(150m);
        _transactionRepositoryMock.Verify(r => r.AddAsync(
            It.Is<Transaction>(t => t.EventId == request.EventId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessEventAsync_With_Duplicate_EventId_Should_Throw_DuplicateEventException()
    {
        var request = new TransactionEventRequest
        {
            EventId = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
            Type = TransactionType.Credit,
            Amount = 50m,
            OccurredAt = DateTime.UtcNow
        };

        _transactionRepositoryMock.Setup(r => r.ExistsAsync(request.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = async () => await _sut.ProcessEventAsync(request);

        await act.Should().ThrowAsync<DuplicateEventException>();
        _accountRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessEventAsync_With_Unknown_Account_Should_Throw_AccountNotFoundException()
    {
        var request = new TransactionEventRequest
        {
            EventId = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
            Type = TransactionType.Credit,
            Amount = 50m,
            OccurredAt = DateTime.UtcNow
        };

        _transactionRepositoryMock.Setup(r => r.ExistsAsync(request.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _accountRepositoryMock.Setup(r => r.GetByIdAsync(request.AccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);

        var act = async () => await _sut.ProcessEventAsync(request);

        await act.Should().ThrowAsync<AccountNotFoundException>();
    }

    [Fact]
    public async Task ProcessEventAsync_With_Insufficient_Balance_Should_Throw_And_Not_Persist()
    {
        var account = new Account(Guid.NewGuid(), "Teste", 10m);
        var request = new TransactionEventRequest
        {
            EventId = Guid.NewGuid(),
            AccountId = account.Id,
            Type = TransactionType.Debit,
            Amount = 100m,
            OccurredAt = DateTime.UtcNow
        };

        _transactionRepositoryMock.Setup(r => r.ExistsAsync(request.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _accountRepositoryMock.Setup(r => r.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var act = async () => await _sut.ProcessEventAsync(request);

        await act.Should().ThrowAsync<InsufficientBalanceException>();
        _transactionRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    public void Dispose() => _context.Dispose();
}