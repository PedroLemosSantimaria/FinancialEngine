using FinancialEngine.Api.DTO;
using FinancialEngine.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinancialEngine.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<TransactionResponse>> Post(
        [FromBody] TransactionEventRequest request, CancellationToken ct)
    {
        var result = await _transactionService.ProcessEventAsync(request, ct);
        return CreatedAtAction(nameof(Post), new { result.EventId }, result);
    }

    [HttpGet("~/api/accounts/{accountId:guid}/transactions")]
    [ProducesResponseType(typeof(PagedResult<TransactionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<TransactionResponse>>> GetStatement(
        Guid accountId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 10 : pageSize;

        var result = await _transactionService.GetStatementAsync(accountId, page, pageSize, ct);
        return Ok(result);
    }
}