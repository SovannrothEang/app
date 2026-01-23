using CoreAPI.DTOs;
using CoreAPI.DTOs.Customers;
using CoreAPI.DTOs.Tenants;
using CoreAPI.Exceptions;
using CoreAPI.Services.Interfaces;
using CoreAPI.Validators.Tenant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Tenants")]
[RequireHttps]
public class TenantsController(
    IAuthorizationService authorizationService,
    ICustomerService customerService,
    ITransactionService transactionService,
    ITenantService tenantService,
    ITransactionTypeService transactionTypeService) : ControllerBase
{
    #region Private Fields
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly ICustomerService _customerService = customerService;
    private readonly ITransactionService _transactionService = transactionService;
    private readonly ITransactionTypeService _transactionTypeService = transactionTypeService;
    private readonly ITenantService _tenantService = tenantService;
    #endregion

    /// <summary>
    /// Get all available tenants in pagination result
    /// </summary>
    [HttpGet]
    [Authorize(Policy = Constants.PlatformRootAccessPolicy)]
    public async Task<ActionResult> GetAllTenantsAsync(
        [FromQuery] PaginationOption option,
        CancellationToken ct = default)
    {
        var tenants = await _tenantService.GetPagedResultsAsync(option, cancellationToken: ct);
        return Ok(tenants);
    }
    
    /// <summary>
    /// Get tenant profile by id
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Policy = Constants.PlatformRootAccessPolicy)]
    public async Task<ActionResult> GetTenantByIdAsync(string id, CancellationToken ct = default)
    {
        var tenant = await _tenantService.GetByIdAsync(id, ct);
        return Ok(tenant);
    }

    /// <summary>
    /// Onboarding a new tenant by Admin only
    /// </summary>
    [HttpPost]
    [Authorize(Constants.PlatformRootAccessPolicy)]
    public async Task<ActionResult> CreateTenantAsync(
        [FromBody] TenantOnBoardingDto dto,
        CancellationToken ct = default)
    {
        var validator = new TenantCreateDtoValidator();
        var result = validator.Validate(dto);
        if (!result.IsValid)
            return BadRequest(result.Errors);

        var (tenant, userId, token) = await _tenantService.CreateAsync(dto, ct);
        return Ok(new
        {
            Tenant = tenant,
            UserId = userId,
            Token = token
        });
    }
    
    /// <summary>
    /// Deactivate a tenant
    /// </summary>
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
    
    // TODO: for customers in this tenant endpoints, need to be pagination, for filtering, ordering, and paged result
    
    /// <summary>
    /// Get all customers who have accounts with tenant, with its last transaction
    /// </summary>
    [HttpGet("{tenantId}/customers")]
    [Authorize(Constants.TenantScopeAccessPolicy)]
    public async Task<ActionResult> GetAllCustomerInTenantScopeAsync(
        [FromRoute] string tenantId,
        [FromQuery] bool? childIncluded,
        [FromQuery] PaginationOption option,
        CancellationToken ct = default)
    {
        childIncluded ??= false;
        var customers = await _customerService.GetPaginatedResultsForTenantAsync(option, childIncluded.Value, ct);
        return Ok(customers);
    }
    
    /// <summary>
    /// Get specific customer that has accounts, and its last transaction, with the tenant, and the _links for operations can be done to a customer's account
    /// TODO: actually need more detail for customer profile
    /// </summary>
    [HttpGet("{tenantId}/customers/{customerId}")]
    [Authorize(Constants.TenantScopeAccessPolicy)]
    public async Task<ActionResult> GetCustomerByIdInTenantScopeAsync(
        [FromRoute] string tenantId,
        [FromRoute] string customerId,
        [FromQuery] bool? childIncluded,
        CancellationToken ct = default)
    {
        childIncluded ??= false;
        var customer = await _customerService.GetByIdForTenantAsync(customerId, childIncluded.Value, ct);
        var operations = await _transactionTypeService.GetAllOperationsAsync(ct);
        return Ok(new
        {
            customer,
            _links = operations.Select(o => new
            {
                rel = o.Slug,
                href = "/api/tenants/{tenantId}/customers/{customerId}/accountTypes/{accountTypeId}/" + o.Slug,
                method = "POST"
            })
        });
    }

    /// <summary>
    /// Using for operations on account, like earn point, redeem or even adjust by the tenant owner
    /// </summary>
    [HttpPost("{tenantId}/customers/{customerId}/accountTypes/{accountTypeId}/{slug}")]
    [Authorize(Constants.TenantScopeAccessPolicy)]
    public async Task<ActionResult> CustomerEarnPointsAsync(
        [FromRoute] string tenantId,
        [FromRoute] string customerId,
        [FromRoute] string accountTypeId,
        [FromRoute] string slug,
        [FromBody] PostTransactionDto dto,
        CancellationToken ct = default)
    {
        try
        {
            var (balance, transactionDetail, tenantDto) = await _transactionService.PostTransactionAsync(
                customerId,
                tenantId, 
                accountTypeId,
                slug,  
                dto,
                ct);
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
    
    // No need customers to involved to actually know what we can do
    [HttpGet("{tenantId}/customers/operations")]
    [Authorize(Constants.TenantScopeAccessPolicy)]
    public async Task<ActionResult> GetAllTransactionTypeAsync(
        [FromRoute] string tenantId,
        CancellationToken ct = default)
    {
        var result = await _transactionTypeService.GetAllOperationsAsync(ct);
        return Ok(result);
    }

    /// <summary>
    /// Checking the customer balance
    /// TODO: there are account type, so should be better if return with accounts
    /// </summary>
    [HttpGet("{tenantId}/customers/{customerId}/balance")]
    [Authorize(Constants.TenantScopeAccessPolicy)]
    public async Task<ActionResult> GetCustomerBalanceByIdAsync(
        [FromRoute] string tenantId,
        [FromRoute] string customerId,
        [FromQuery] PaginationOption option,
        [FromQuery] bool? childIncluded,
        CancellationToken ct = default)
    {
        childIncluded ??= false;
        var (balance, result) = await _customerService.GetCustomerBalanceByIdAsync(customerId, tenantId, option, childIncluded.Value, ct);
        return Ok(new
        {
            TotalBalance = balance,
            result
        });
    }
}