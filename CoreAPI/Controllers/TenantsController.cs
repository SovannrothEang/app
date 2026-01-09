using AutoMapper;
using CoreAPI.DTOs.Customers;
using CoreAPI.DTOs.Tenants;
using CoreAPI.Exceptions;
using CoreAPI.Services.Interfaces;
using CoreAPI.Validators.Customers;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Tenants")]
[RequireHttps]
public class TenantsController(
    IAuthorizationService authorizationService,
    IUserService userService,
    ICustomerService customerService,
    ITransactionService transactionService,
    IMapper mapper,
    ILogger<TenantsController> logger,
    ITenantService tenantService,
    ITransactionTypeService transactionTypeService) : ControllerBase
{
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly IUserService _userService = userService;
    private readonly ICustomerService _customerService = customerService;
    private readonly ITransactionService _transactionService = transactionService;
    private readonly ITransactionTypeService _transactionTypeService = transactionTypeService;
    private readonly ITenantService _tenantService = tenantService;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<TenantsController> _logger = logger;

    /// <summary>
    /// Get all available tenants
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    [HttpGet]
    [Authorize(Policy = Constants.PlatformRootAccessPolicy)]
    public async Task<ActionResult> GetAllTenantsAsync(CancellationToken ct = default)
    {
        var tenants = await _tenantService.GetAllAsync(cancellationToken: ct);
        return Ok(tenants);
    }
    
    /// <summary>
    /// Get tenant profile by id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize(Policy = Constants.PlatformRootAccessPolicy)]
    public async Task<ActionResult> GetTenantByIdAsync(string id, CancellationToken ct = default)
    {
        using (_logger.BeginScope("Tenants retrieval operation for id: {tenantId}", id))
        {
            var tenant = await _tenantService.GetByIdAsync(id, ct);
            return Ok(_mapper.Map<TenantDto>(tenant));
        }
    }

    /// <summary>
    /// Onboarding a tenant
    /// </summary>
    /// <param name="dto"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize(Constants.PlatformRootAccessPolicy)]
    public async Task<ActionResult> CreateTenantAsync(
        [FromBody] TenantCreateDto dto,
        CancellationToken ct = default)
    {
        var (userId, token) = await _userService.CreateTenantAndUserAsync(dto, ct);
        return Ok(new
        {
            UserId = userId,
            Token = token
        });
    }
    
    /// <summary>
    /// Deactivate a tenant
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    [HttpPut("{tenantId}/deactivate")]
    [Authorize(Constants.PlatformRootAccessPolicy)]
    public async Task<ActionResult> DeactivateTenantAsync(
        [FromRoute] string tenantId,
        CancellationToken ct = default)
    {
        await _tenantService.DeactivateAsync(tenantId, ct);
        return NoContent();
    }
    
    /// <summary>
    /// Activate a tenant
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    [HttpPut("{tenantId}/activate")]
    [Authorize(Policy = Constants.PlatformRootAccessPolicy)]
    public async Task<ActionResult> ActivateTenantAsync(
        [FromRoute] string tenantId,
        CancellationToken ct = default)
    {
        var tenantAccess = await _authorizationService.AuthorizeAsync(User, tenantId);
        if (!tenantAccess.Succeeded)
            return Forbid();
        
        await _tenantService.ActivateAsync(tenantId, ct);
        return NoContent();
    }
    
    [HttpGet("{tenantId}/customers")]
    [Authorize(Constants.TenantScopeAccessPolicy)]
    public async Task<ActionResult> GetAllCustomerInTenantScopeAsync(
        [FromRoute] string tenantId,
        [FromQuery] bool? childIncluded = false, // won't include transactions
        CancellationToken ct = default)
    {
        childIncluded ??= false;
        var customers = await _customerService.GetCustomersPerTenantAsync(childIncluded.Value, ct);
        return Ok(customers);
    }
    
    [HttpGet("{tenantId}/customers/{customerId}")]
    [Authorize(Constants.TenantScopeAccessPolicy)]
    public async Task<ActionResult> GetCustomerByIdInTenantScopeAsync(
        [FromRoute] string tenantId,
        [FromRoute] string customerId,
        [FromQuery] bool? childIncluded = false, // won't include transactions
        CancellationToken ct = default)
    {
        // Example:
        // {
        //   "id": "cust_001",
        //   "name": "John Doe",
        //   "balance": 500,
        //   "_links": [
        //     {
        //       "rel": "earn",
        //       "href": "/api/tenants/{id}/customers/{id}/earn",
        //       "method": "POST"
        //     },
        //     {
        //       "rel": "redeem",
        //       "href": "/api/tenants/{id}/customers/{id}/redeem",
        //       "method": "POST"
        //     }
        //   ]
        // }
        childIncluded ??= false;
        var customer = await _customerService.GetByIdInTenantScopeAsync(customerId, childIncluded.Value, ct);
        var operations = await _transactionTypeService.GetAllOperationsAsync(ct);
        return Ok(new
        {
            customer,
            _links = operations.Select(o => new
            {
                rel = o.Slug,
                href = "/api/tenants/{tenantId}/customers/{customerId}" + o.Slug,
                method = "POST"
            })
        });
    }

    /// <summary>
    /// Earn point route
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="customerId"></param>
    /// <param name="slug"></param>
    /// <param name="dto"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    [HttpPost("{tenantId}/customers/{customerId}/{slug}")]
    [Authorize(Constants.TenantScopeAccessPolicy)]
    public async Task<ActionResult> CustomerEarnPointsAsync(
        [FromRoute] string tenantId,
        [FromRoute] string customerId,
        [FromRoute] string slug,
        [FromBody] CustomerPostTransaction dto,
        CancellationToken ct = default)
    {
        try
        {
            var (balance, transactionDetail, tenantDto) = await _transactionService.PostTransactionAsync(customerId, tenantId, slug,  dto, ct);
            return Ok(new
            {
                Balance = balance,
                TransactionDetail = transactionDetail,
                Tenant = tenantDto
            });
        }
        catch (InsufficientBalanceException ex)
        {
            return BadRequest(new
            {
                ErrorCode = InsufficientBalanceException.Code,
                Balance = ex.Amount,
                RequestBalance = ex.RequestedAmount
            });
        }
    }
    
    // TODO: I think of moving this to /api/tenants/{tenantId}/operations
    // No need customers to involved to actually know what we can do
    [HttpGet("{tenantId}/customers/{customerId}/operations")]
    [Authorize(Constants.TenantScopeAccessPolicy)]
    public async Task<ActionResult> GetAllTransactionTypeAsync(
        [FromRoute] string tenantId,
        [FromRoute] string customerId,
        CancellationToken ct = default)
    {
        // Example:
        // [
        //   {
        //     "slug": "earn",
        //     "name": "Points Earn",
        //     "url": "/api/tenants/123/customers/456/earn" 
        //   },
        //   {
        //     "slug": "redeem",
        //     "name": "Points Redemption",
        //     "url": "/api/tenants/123/customers/456/redeem"
        //   }
        // ]
        var result = await _transactionTypeService.GetAllOperationsAsync(ct);
        return Ok(result);
    }
    
    /// <summary>
    /// Checking the customer balance
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="customerId"></param>
    /// <param name="options"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    [HttpGet("{tenantId}/customers/{customerId}/balance")]
    [Authorize(Constants.TenantCustomerAccessPolicy)]
    public async Task<ActionResult> GetCustomerBalanceByIdAsync(
        [FromRoute] string tenantId,
        [FromRoute] string customerId,
        [FromQuery] CustomerGetBalanceOptionsDto? options,
        CancellationToken ct = default)
    {
        var (balance, lastActivities) = await _customerService.GetCustomerBalanceByIdAsync(customerId, tenantId, options, ct);
        return Ok(new
        {
            Balance = balance,
            TotalTransactions = lastActivities.Count,
            LastActivities =  lastActivities
        });
    }
}