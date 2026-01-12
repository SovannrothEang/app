using AutoMapper;
using CoreAPI.DTOs;
using CoreAPI.DTOs.Customers;
using CoreAPI.DTOs.Tenants;
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

    /// <summary>
    /// Get all customers by a SuperAdmin, or root access
    /// </summary>
    /// <param name="childIncluded"></param>
    /// <param name="ct"></param>
    /// <returns>List of customer dto</returns>
    [HttpGet]
    [Authorize(Policy = Constants.PlatformRootAccessPolicy)]
    public async Task<ActionResult> GetAllCustomersAsync(
        bool? childIncluded = false,
        CancellationToken ct = default)
    {
        childIncluded ??= false;
        var customers = await _customerService.GetAllAsync(childIncluded.Value, ct);
        return Ok(customers);
    }
    
    /// <summary>
    /// Get customer by id (for global usage, can be SuperAdmin retrieving, or customer themselves)
    /// </summary>
    /// <param name="id"></param>
    /// <param name="childIncluded"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>Customer Dto</returns>
    [HttpGet("{id}")]
    [ActionName(nameof(GetCustomerByIdAsync))]
    [Authorize(Policy = Constants.CustomerAccessPolicy)]
    public async Task<ActionResult> GetCustomerByIdAsync(
        [FromRoute] string id,
        [FromQuery] bool? childIncluded = false,
        CancellationToken cancellationToken = default)
    {
        childIncluded ??= false;
        var customer = await _customerService.GetByIdForCustomerAsync(id, childIncluded.Value, cancellationToken);
        return Ok(_mapper.Map<CustomerDto>(customer));
    }

    /// <summary>
    /// Get all transactions of all customers,
    /// </summary>
    /// <param name="childIncluded"></param>
    /// <param name="option"></param>
    /// <param name="ct"></param>
    /// <returns>List of transactions</returns>
    [HttpGet("transactions")]
    [Authorize(Policy = Constants.PlatformRootAccessPolicy)]
    public async Task<ActionResult> GetAllCustomersTransactionsAsync(
        [FromQuery] bool? childIncluded,
        [FromQuery] PaginationOption option,
        CancellationToken ct = default)
    {
        childIncluded ??= false;
        var transactions = await _transactionService.GetAllTransactionsAsync(option, childIncluded.Value, ct);
        return Ok(transactions);
    }

    /// <summary>
    /// Get all transactions of a customer by id
    /// </summary>
    /// <param name="customerId"></param>
    /// <param name="ct"></param>
    /// <returns>List of transactions</returns>
    [HttpGet("{customerId}/transactions")]
    [Authorize(Policy = Constants.CustomerAccessPolicy)]
    public async Task<ActionResult> GetCustomerTransactionsByIdAsync(string customerId, CancellationToken ct = default)
    {
        var transactions = await _transactionService.GetAllByCustomerAsync(customerId, ct);
        return Ok(transactions);
    }
    
    [HttpGet("{customerId}/overview")]
    [Authorize(Policy = Constants.CustomerAccessPolicy)]
    public async Task<ActionResult> GetCustomerDashboard(
        [FromRoute] string customerId,
        [FromQuery] bool? childIncluded,
        [FromQuery] string? tenantId = null,
        [FromQuery] string? tenantName = null,
        [FromQuery] decimal? balance = null,
        [FromQuery] DateTime? lastActivity = null,
        CancellationToken ct = default)
    {
        childIncluded ??= false;
        var customer = await _customerService.GetByIdForCustomerAsync(customerId, childIncluded.Value, ct);
        var accounts = await _accountService.GetAllWithCustomerAsync(customerId, childIncluded.Value, ct);

        var tenants = accounts
            .GroupBy(a => a.TenantId)
            .Select(group => new
            {
                TenantId = group.Key,
                TenantName = group.First().Tenant!.Name,
                TotalTenantBalance = group.Sum(acc => acc.Balance),
                Accounts = group.Select(acc => new
                {
                    AccountType = acc.AccountType,
                    CurrentBalance = acc.Balance,
                })
            }).ToList();
        
        return Ok(new
        {
            TotolPoint = accounts.Select(acc => acc.Balance).Sum(),
            AllTenants = tenants
        });
        // CustomerProfile,
        // Tenant
        // TenantId, TenantName, Point, Account
    }
}