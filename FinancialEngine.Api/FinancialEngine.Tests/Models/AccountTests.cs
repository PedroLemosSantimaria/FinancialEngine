using FinancialEngine.Api.Exceptions;
using FinancialEngine.Api.Models;
using FinancialEngine.Api.Models.Enums;
using FluentAssertions;
using Xunit;

namespace FinancialEngine.Tests.Models;

public class AccountTests
{
    [Fact]
    public void Apply_Credit_Should_Increase_Balance()
    {
        var account = new Account(Guid.NewGuid(), "Teste", 100m);
        var eventId = Guid.NewGuid();

        var transaction = account.Apply(eventId, TransactionType.Credit, 50m, DateTime.UtcNow);

        account.Balance.Should().Be(150m);
        transaction.BalanceAfter.Should().Be(150m);
        transaction.EventId.Should().Be(eventId);
    }

    [Fact]
    public void Apply_Debit_With_Sufficient_Balance_Should_Decrease_Balance()
    {
        var account = new Account(Guid.NewGuid(), "Teste", 100m);

        var transaction = account.Apply(Guid.NewGuid(), TransactionType.Debit, 40m, DateTime.UtcNow);

        account.Balance.Should().Be(60m);
        transaction.BalanceAfter.Should().Be(60m);
    }

    [Fact]
    public void Apply_Debit_Greater_Than_Balance_Should_Throw_InsufficientBalanceException()
    {
        var account = new Account(Guid.NewGuid(), "Teste", 30m);

        var act = () => account.Apply(Guid.NewGuid(), TransactionType.Debit, 50m, DateTime.UtcNow);

        act.Should().Throw<InsufficientBalanceException>();
        account.Balance.Should().Be(30m);
    }

    [Fact]
    public void Apply_Debit_Equal_To_Balance_Should_Result_In_Zero()
    {
        var account = new Account(Guid.NewGuid(), "Teste", 50m);

        account.Apply(Guid.NewGuid(), TransactionType.Debit, 50m, DateTime.UtcNow);

        account.Balance.Should().Be(0m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Apply_With_NonPositive_Amount_Should_Throw(decimal amount)
    {
        var account = new Account(Guid.NewGuid(), "Teste", 100m);

        var act = () => account.Apply(Guid.NewGuid(), TransactionType.Credit, amount, DateTime.UtcNow);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}