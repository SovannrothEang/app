using CoreAPI.DTOs;
using CoreAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[RequireHttps]
public class CustomersController(
    ITransactionService transactionService,
    IAccountService accountService,
    ICustomerService customerService,
    ILogger<CustomersController> logger) : ControllerBase
{
    #region Private Fields
    private readonly ITransactionService _transactionService = transactionService;
    private readonly IAccountService _accountService = accountService;
    private readonly ICustomerService _customerService = customerService;
    private readonly ILogger<CustomersController> _logger = logger;
    #endregion

    /// <summary>
    /// Get all customers by the Admin
    /// </summary>
    [HttpGet]
    [Authorize(Policy = Constants.PlatformRootAccessPolicy)]
    public async Task<ActionResult> GetAllCustomersAsync(
        [FromQuery] bool? childIncluded,
        [FromQuery] PaginationOption option,
        CancellationToken ct = default)
    {
        childIncluded ??= false;
        var customers = await _customerService.GetPaginatedResultsAsync(option, childIncluded.Value, ct);
        return Ok(customers);
    }
    
    /// <summary>
    /// Get the customer profile by Customer, with all of accounts, and its last transaction
    /// TODO: why need this when we have the Overview or Dashboard??
    /// </summary>
    [HttpGet("{id}")]
    [ActionName(nameof(GetCustomerByIdAsync))]
    [Authorize(Policy = Constants.CustomerAccessPolicy)]
    public async Task<ActionResult> GetCustomerByIdAsync(
        [FromRoute] string id,
        [FromQuery] bool? childIncluded,
        CancellationToken cancellationToken = default)
    {
        childIncluded ??= false;
        var customer = await _customerService.GetByIdForGlobalAsync(id, childIncluded.Value, cancellationToken);
        return Ok(customer);
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
        var transactions = await _transactionService.GetPagedResultAsync(option, childIncluded.Value, ct);
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
        [FromQuery] bool? childIncluded,
        CancellationToken ct = default)
    {
        using (_logger.BeginScope("GetCustomerTransactionsByIdAsync for CustomerId: {CustomerId}", customerId))
        {
            _logger.LogInformation("Retrieving transactions for customer.");
            childIncluded ??= false;
            var transactions = await _transactionService.GetAllByCustomerIdForGlobalAsync(customerId, option, childIncluded.Value, ct);
            return Ok(transactions);
        }
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
        var (totalBalance, result) = await _accountService.GetAllByCustomerIdForGlobalAsync(customerId, option, ct);
        return Ok(new
        {
            TotalBalance = totalBalance,
            TenantProfiles = result
        });
    }
}