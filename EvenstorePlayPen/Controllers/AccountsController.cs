using EvenstorePlayPen.Models;
using EvenstorePlayPen.Services;
using Microsoft.AspNetCore.Mvc;

namespace EvenstorePlayPen.Controllers;

[Route("api/v1/account")]
public class AccountsController : ControllerBase
{
    private readonly ILogger<AccountsController> _logger;
    private readonly AccountService _accountService;

    public AccountsController(ILogger<AccountsController> logger, AccountService accountService)
    {
        _logger = logger;
        _accountService = accountService;
    }
    
    [HttpPost]
    [Route("add")]
    public async Task<IActionResult> AddAccount(Account account)
    {
        account.Id = Guid.NewGuid();
        await _accountService.AddAccount(account);
        return Ok();
    }
    
    [HttpPut]
    [Route("update")]
    public async Task<IActionResult> UpdateAccount(Account account)
    {
        await _accountService.UpdateAccount(account);
        return Ok();
    }
    
    [HttpGet]
    [Route("get")]
    public async Task<IActionResult> GetAll()
    {
        var accounts = await _accountService.GetAccounts();
        return Ok(accounts);
    }
    
    [HttpGet]
    [Route("get-by-id")]
    public async Task<IActionResult> Get(Guid id)
    {
        var accounts = await _accountService.GetAccount(id);
        return Ok(accounts);
    }
    
    [HttpDelete]
    [Route("Delete")]
    public async Task<IActionResult> DeleteAccount(Guid id)
    {
        await _accountService.DeleteAccount(id);
        return Ok();
    }

    [HttpDelete]
    [Route("delete-stream")]
    public async Task<IActionResult> DeleteStream(string streamName)
    {
        await _accountService.DeleteStream(streamName);
        return Ok();
    }
}