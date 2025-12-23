using AutoMapper;
using CoreAPI.DTOs.Customers;
using CoreAPI.DTOs.Tenants;
using CoreAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CoreAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[RequireHttps]
public class CustomersController(
    IMapper mapper,
    IPointTransactionService transactionService,
    ITenantService tenantService,
    ICustomerService customerService,
    ILoyaltyAccountService accountService) : ControllerBase
{
    private readonly IMapper _mapper = mapper;
    private readonly IPointTransactionService _transactionService = transactionService;
    private readonly ILoyaltyAccountService _accountService = accountService;
    private readonly ITenantService _tenantService = tenantService;
    private readonly ICustomerService _customerService = customerService;
    
    [HttpGet]
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
    
    [HttpPost]
    public async Task<ActionResult> CreateCustomerAsync(
        [FromBody] CustomerCreateDto dto,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(
            nameof(GetCustomerByIdAsync),
            new { id = customer.Id },
            customer);
    }
    
    [HttpGet("transactions")]
    public async Task<ActionResult> GetAllCustomersTransactionsAsync(CancellationToken ct = default)
    {
        var transactions = await _transactionService.GetAllTransactionsAsync(ct);
        return Ok(transactions);
    }

    [HttpGet("{customerId}/transactions")]
    public ActionResult GetCustomerTransactionsByIdAsync(string customerId, CancellationToken ct = default)
    {
        return Ok();
    }
    
    [HttpGet("{customerId}/overview")]
    public async Task<ActionResult> GetCustomerDashboard(
        [FromRoute] string customerId,
        [FromQuery] string? tenantId = null,
        [FromQuery] string? tenantName = null,
        [FromQuery] decimal? balance = null,
        [FromQuery] DateTime? lastActivity = null,
        CancellationToken ct = default)
    {
        var accounts = await _accountService.GetAllWithCustomerAsync(customerId, ct);
        
        accounts = accounts.ToList();
        var list = new List<TenantLevelDto>();
        var tenantIds = accounts.Select(account => account.TenantId);
        foreach (var id in tenantIds)
        {
            var tenant = await _tenantService.GetByIdAsync(id, ct);
            if (tenant == null) continue;
            list.Add(new TenantLevelDto(
                tenant.Id,
                tenant.Name,
                accounts.First(a => a.TenantId == tenant.Id).Balance));
        }
        return Ok(new
        {
            TotolPoint = accounts.Select(t => t.Balance).Sum(),
            AllTenants = list,
        });
    }
}