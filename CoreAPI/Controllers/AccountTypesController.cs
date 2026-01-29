using CoreAPI.DTOs.Accounts;
using CoreAPI.Services.Interfaces;
using CoreAPI.Validators.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[RequireHttps]
[Authorize(Policy = Constants.TenantScopeAccessPolicy)]
public class AccountTypesController(
    IAccountTypeService accountTypeService,
    ICurrentUserProvider currentUserProvider,
    ILogger<AccountTypesController> logger) : ControllerBase
{
    private readonly IAccountTypeService _accountTypeService = accountTypeService;
    private readonly ICurrentUserProvider _currentUserProvider = currentUserProvider;
    private readonly ILogger<AccountTypesController> _logger = logger;

    [HttpGet]
    public async Task<IActionResult> GetAccountTypes(CancellationToken cancellationToken = default)
    {
        var types =  await _accountTypeService.GetAllAsync(cancellationToken);
        return Ok(types);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAccountTypeById(string id, CancellationToken cancellationToken = default)
    {
        var type = await _accountTypeService.GetByIdAsync(id, cancellationToken);
        return Ok(type);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAccountType(
        [FromBody] AccountTypeCreateDto dto,
        CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("[AccountTypesController] Create account type by User {Performer}", _currentUserProvider.UserId);
        var result = new AccountTypeCreateDtoValidator().Validate(dto);
        if (result.IsValid)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("Invalid account type creation attempt: {Errors} by User {Performer}",
                    result.Errors, _currentUserProvider.UserId);
            return BadRequest(result.Errors);
        }
        var type = await _accountTypeService.CreateAsync(dto, cancellationToken);
        return Ok(type);
    }
}
