using AutoMapper;
using CoreAPI.DTOs.Auth;
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
    ICustomerService customerService) : ControllerBase
{
    private readonly IMapper _mapper = mapper;
    private readonly ITransactionService _transactionService = transactionService;
    private readonly ICustomerService _customerService = customerService;
    
    [HttpGet]
    [Authorize(Policy = Constants.TenantScopeAccessPolicy)]
    public async Task<ActionResult> GetCustomersAsync(
        [FromQuery] bool? childIncluded = false, 
        CancellationToken ct = default)
    {
        childIncluded ??= false;
        var customers = await _customerService.GetAllAsync(childIncluded.Value, ct);
        return Ok(customers.Select(c => _mapper.Map<CustomerDto>(c)));
    }

    [HttpGet("{id}")]
    [ActionName(nameof(GetCustomerByIdAsync))]
    [Authorize(Policy = Constants.TenantScopeAccessPolicy)]
    public async Task<ActionResult> GetCustomerByIdAsync(
        [FromRoute] string id,
        [FromQuery] bool? childIncluded = false,
        CancellationToken cancellationToken = default)
    {
        childIncluded ??= false;
        var customer = await _customerService.GetByIdAsync(id, childIncluded.Value, cancellationToken);
        if (customer == null) return NotFound();
        return Ok(_mapper.Map<CustomerDto>(customer));
    }
    
    [HttpGet("transactions")]
    [Authorize(Policy = Constants.PlatformRootPolicy)]
    public async Task<ActionResult> GetAllCustomersTransactionsAsync(CancellationToken ct = default)
    {
        // TODO: make sure that this route return all of the transactions
        var transactions = await _transactionService.GetAllTransactionsAsync(ct);
        return Ok(transactions);
    }

    [HttpGet("{customerId}/transactions")]
    [Authorize(Policy = Constants.TransactionAccessPolicy)]
    public async Task<ActionResult> GetCustomerTransactionsByIdAsync(string customerId, CancellationToken ct = default)
    {
        var transactions = await _transactionService.GetByCustomerIdAsync(customerId, ct);
        return Ok(transactions);
    }
    
    // TODO: Fix Load customer dashboard
    // [HttpGet("{customerId}/overview")]
    // public async Task<ActionResult> GetCustomerDashboard(
    //     [FromRoute] string customerId,
    //     [FromQuery] string? tenantId = null,
    //     [FromQuery] string? tenantName = null,
    //     [FromQuery] decimal? balance = null,
    //     [FromQuery] DateTime? lastActivity = null,
    //     CancellationToken ct = default)
    // {
    //     var customer = await _customerService.GetByIdAsync(customerId, childIncluded: true, ct);
    //     if (customer == null) return NotFound();
    //     
    //     var list = new List<TenantLevelDto>();
    //     var ids = accounts.Select(account => account.TenantId);
    //     foreach (var id in ids)
    //     {
    //         var tenant = await _tenantService.GetByIdAsync(id, ct);
    //         if (tenant == null) continue;
    //         list.Add(new TenantLevelDto(
    //             tenant.Id,
    //             tenant.Name,
    //             accounts.First(a => a.TenantId == tenant.Id).Balance));
    //     }
    //     return Ok(new
    //     {
    //         TotolPoint = customer.LoyaltyAccounts.Select(acc => acc.Balance).Sum(),
    //         AllTenants = list,
    //     });
    // }
}