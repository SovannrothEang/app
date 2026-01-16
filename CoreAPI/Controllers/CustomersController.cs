using AutoMapper;
using CoreAPI.DTOs;
using CoreAPI.DTOs.Customers;
using CoreAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[RequireHttps]
public class CustomersController(
    IMapper mapper,
    ITransactionService transactionService,
    IAccountService accountService,
    ICustomerService customerService) : ControllerBase
{
    private readonly IMapper _mapper = mapper;
    private readonly ITransactionService _transactionService = transactionService;
    private readonly IAccountService _accountService = accountService;
    private readonly ICustomerService _customerService = customerService;

    [HttpGet]
    [Authorize(Policy = Constants.PlatformRootAccessPolicy)]
    public async Task<ActionResult> GetAllCustomersAsync(
        bool? childIncluded = false,
        CancellationToken ct = default)
    {
        childIncluded ??= false;
        var customers = await _customerService.GetAllForGobalAsync(childIncluded.Value, ct);
        return Ok(customers);
    }
    
    [HttpGet("{id}")]
    [ActionName(nameof(GetCustomerByIdAsync))]
    [Authorize(Policy = Constants.CustomerAccessPolicy)]
    public async Task<ActionResult> GetCustomerByIdAsync(
        [FromRoute] string id,
        [FromQuery] bool? childIncluded,
        CancellationToken cancellationToken = default)
    {
        childIncluded ??= false;
        var customer = await _customerService.GetByIdForCustomerAsync(id, null, childIncluded.Value, cancellationToken);
        return Ok(_mapper.Map<CustomerDto>(customer));
    }

    /// <summary>
    /// Expecting a pagination result of transactions, all customer transaction, which retrieve by SuperAdmin, or related role (future implement)
    /// </summary>
    [HttpGet("transactions")]
    [Authorize(Policy = Constants.PlatformRootAccessPolicy)]
    public async Task<ActionResult> GetAllCustomersTransactionsAsync(
        [FromQuery] PaginationOption option,
        [FromQuery] bool? childIncluded,
        CancellationToken ct = default)
    {
        childIncluded ??= false;
        var transactions = await _transactionService.GetAllAsync(option, childIncluded.Value, ct);
        return Ok(transactions);
    }

    /// <summary>
    /// Expecting a pagination result of transaction, which retrieve by Customer owner or SuperAdmin
    /// </summary>
    [HttpGet("{customerId}/transactions")]
    [Authorize(Policy = Constants.CustomerAccessPolicy)]
    public async Task<ActionResult> GetCustomerTransactionsByIdAsync(
        string customerId,
        [FromQuery] PaginationOption option,
        [FromQuery] bool? childIncluded = false,
        CancellationToken ct = default)
    {
        childIncluded ??= false;
        var transactions = await _transactionService.GetAllByCustomerIdForGlobalAsync(customerId, option, childIncluded.Value, ct);
        return Ok(transactions);
    }
    
    /// <summary>
    /// Customer dashboard, including tenant profile, a list of account details, and transactions (default: last activity)
    /// </summary>
    [HttpGet("{customerId}/overview")]
    [Authorize(Policy = Constants.CustomerAccessPolicy)]
    public async Task<ActionResult> GetCustomerDashboard(
        [FromRoute] string customerId,
        [FromQuery] PaginationOption option,
        CancellationToken ct = default)
    {
        var (totalBalance, result) = await _accountService.GetAllByCustomerIdForGlobalAsync(customerId, option, ct); // TenantId is for filtering
        return Ok(new
        {
            TotalBalance = totalBalance,
            TenantProfiles = result
        });
    }
}