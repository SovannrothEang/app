using AutoMapper;
using CoreAPI.DTOs.Customers;
using CoreAPI.DTOs.Tenants;
using CoreAPI.Exceptions;
using CoreAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Tenants")]
[RequireHttps]
public class TenantsController(
    ICurrentUserProvider currentUserProvider,
    IAuthorizationService authorizationService,
    IUserService userService,
    ICustomerService customerService,
    ITransactionService transactionService,
    IMapper mapper,
    ILogger<TenantsController> logger,
    ITenantService tenantService) : ControllerBase
{
    private readonly ICurrentUserProvider _currentUserProvider = currentUserProvider;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly IUserService _userService = userService;
    private readonly ICustomerService _customerService = customerService;
    private readonly ITransactionService _transactionService = transactionService;
    private readonly ITenantService _tenantService = tenantService;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<TenantsController> _logger = logger;

    [HttpGet]
    [Authorize(Policy = Constants.PlatformRootAccessPolicy)]
    public async Task<ActionResult> GetAllTenantsAsync(CancellationToken ct = default)
    {
        var tenants = await _tenantService.GetAllAsync(cancellationToken: ct);
        return Ok(tenants);
    }
    
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
    
    [HttpPut("{tenantId}/deactivate")]
    [Authorize(Constants.PlatformRootAccessPolicy)]
    public async Task<ActionResult> DeactivateTenantAsync(
        [FromRoute] string tenantId,
        CancellationToken ct = default)
    {
        await _tenantService.DeactivateAsync(tenantId, ct);
        return NoContent();
    }
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

    [HttpPost("{tenantId}/customers/{customerId}/earn")]
    [Authorize(Constants.TenantCustomerAccessPolicy)]
    public async Task<ActionResult> CustomerEarnPointsAsync(
        [FromRoute] string tenantId,
        [FromRoute] string customerId,
        [FromBody] CustomerEarnPointDto dto,
        CancellationToken ct = default)
    {
        var (balance, transactionDetail) = await _transactionService.EarnPointAsync(customerId, tenantId, dto, ct);
        return Ok(new
        {
            Balance = balance,
            TransactionDetail = transactionDetail,
        });
    }
    
    [HttpPost("{tenantId}/customers/{customerId}/redeem")]
    [Authorize(Constants.TenantCustomerAccessPolicy)]
    public async Task<ActionResult> CustomerRedeemPointsAsync(
        [FromRoute] string tenantId,
        [FromRoute] string customerId,
        [FromBody] CustomerRedeemPointDto dto,
        CancellationToken ct = default)
    {
        try
        {
            var (balance, transactionDetail) = await _transactionService.RedeemPointAsync(customerId, tenantId, dto, ct);
            return Ok(new
            {
                Balance = balance,
                TransactionDetail = transactionDetail,
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
    
    [HttpPost("{tenantId}/customers/{customerId}/adjust")]
    [Authorize(Constants.TenantCustomerAccessPolicy)]
    public async Task<ActionResult> CustomerAdjustmentPointsAsync(
        [FromRoute] string tenantId,
        [FromRoute] string customerId,
        [FromBody] CustomerAdjustPointDto dto,
        CancellationToken ct = default)
    {
        var current = _currentUserProvider.UserId;
        var (balance, transactionDetail) =
            await _transactionService.AdjustPointAsync(customerId, tenantId, current, dto, ct);
        return Ok(new
        {
            Balance = balance,
            TransactionDetail = transactionDetail,
        });
    }

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