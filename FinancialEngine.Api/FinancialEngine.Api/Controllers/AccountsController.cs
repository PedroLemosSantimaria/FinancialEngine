using FinancialEngine.Api.DTO;
using FinancialEngine.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FinancialEngine.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly IAccountRepository _accountRepository;

    public AccountsController(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AccountResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AccountResponse>>> GetAll(CancellationToken ct)
    {
        var accounts = await _accountRepository.GetAllAsync(ct);
        return Ok(accounts.Select(AccountResponse.FromEntity));
    }
}